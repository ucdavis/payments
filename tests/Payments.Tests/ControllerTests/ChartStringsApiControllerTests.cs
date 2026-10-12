using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using AggieEnterpriseApi.Validation;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using Moq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Payments.Core.Data;
using Payments.Core.Domain;
using Payments.Core.Models.Validation;
using Payments.Core.Services;
using Payments.Mvc.Controllers;
using Payments.Mvc.Identity;
using Payments.Mvc.Models.ChartStringApiViewModels;
using Payments.Mvc.Models.Roles;
using Payments.Mvc.Swagger;
using Swashbuckle.AspNetCore.Swagger;
using Xunit;

namespace payments.Tests.ControllerTests
{
    [Trait("Category", "ControllerTests")]
    public class ChartStringsApiControllerTests
    {
        private const string GlChartString = "1234-12345-1234567-123456-12-123-1234567890-123456-0000-000000-000000";
        private const string PpmChartString = "1234567890-123456-1234567-770006";
        private const string SponsoredPpmChartString = "1234567890-123456-1234567-770006-1234567-12345";

        [Theory]
        [InlineData("credit", true)]
        [InlineData("credit", false)]
        [InlineData("dEbIt", true)]
        [InlineData("dEbIt", false)]
        public void RequestValidationAcceptsMixedCaseDirectionsAndAnExplicitFalseFlag(string direction, bool useRechargeValidation)
        {
            var request = CreateRequest(direction, useRechargeValidation);
            var errors = new List<ValidationResult>();

            var valid = Validator.TryValidateObject(request, new ValidationContext(request), errors, validateAllProperties: true);

            Assert.True(valid, string.Join(" ", errors.Select(error => error.ErrorMessage)));
        }

        [Theory]
        [InlineData(GlChartString, FinancialChartStringType.Gl, "Credit", RechargeAccount.CreditDebit.Credit)]
        [InlineData(GlChartString, FinancialChartStringType.Gl, "credit", RechargeAccount.CreditDebit.Credit)]
        [InlineData(GlChartString, FinancialChartStringType.Gl, "Debit", RechargeAccount.CreditDebit.Debit)]
        [InlineData(GlChartString, FinancialChartStringType.Gl, "dEbIt", RechargeAccount.CreditDebit.Debit)]
        [InlineData(PpmChartString, FinancialChartStringType.Ppm, "credit", RechargeAccount.CreditDebit.Credit)]
        [InlineData(PpmChartString, FinancialChartStringType.Ppm, "dEbIt", RechargeAccount.CreditDebit.Debit)]
        [InlineData(SponsoredPpmChartString, FinancialChartStringType.Ppm, "Credit", RechargeAccount.CreditDebit.Credit)]
        [InlineData(SponsoredPpmChartString, FinancialChartStringType.Ppm, "Debit", RechargeAccount.CreditDebit.Debit)]
        public async Task RechargeValidationUsesTheRequestedDirectionAndPreservesTheResult(
            string chartString, FinancialChartStringType chartType, string direction,
            RechargeAccount.CreditDebit expectedDirection)
        {
            Assert.Equal(chartType, FinancialChartValidation.GetFinancialChartStringType(chartString));
            var expected = CreateValidationResult(isValid: true);
            expected.CoaChartType = chartType;
            var service = new Mock<IAggieEnterpriseService>(MockBehavior.Strict);
            service.Setup(item => item.IsRechargeAccountValid(chartString, expectedDirection, true))
                .ReturnsAsync(expected);
            var controller = CreateController(service.Object);

            var result = await controller.Validate(CreateRequest(direction, useRechargeValidation: true, chartString: chartString));

            AssertValidationResult(expected, result.Value);
            service.Verify(item => item.IsRechargeAccountValid(chartString, expectedDirection, true), Times.Once);
            service.VerifyNoOtherCalls();
        }

        [Theory]
        [InlineData(GlChartString, "Credit")]
        [InlineData(GlChartString, "credit")]
        [InlineData(GlChartString, "Debit")]
        [InlineData(GlChartString, "dEbIt")]
        [InlineData(PpmChartString, "credit")]
        [InlineData(PpmChartString, "Debit")]
        [InlineData(SponsoredPpmChartString, "Credit")]
        [InlineData(SponsoredPpmChartString, "dEbIt")]
        public async Task PaymentValidationUsesExistingAccountRulesForEitherDirection(string chartString, string direction)
        {
            var expected = CreateValidationResult(isValid: true);
            expected.CoaChartType = FinancialChartValidation.GetFinancialChartStringType(chartString);
            var service = new Mock<IAggieEnterpriseService>(MockBehavior.Strict);
            service.Setup(item => item.IsAccountValid(chartString, true)).ReturnsAsync(expected);
            var controller = CreateController(service.Object);

            var result = await controller.Validate(CreateRequest(direction, useRechargeValidation: false, chartString: chartString));

            AssertValidationResult(expected, result.Value);
            service.Verify(item => item.IsAccountValid(chartString, true), Times.Once);
            service.VerifyNoOtherCalls();
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async Task InvalidValidationResultsPreserveTheNormalizedChartStringMessagesAndWarnings(bool useRechargeValidation)
        {
            var expected = CreateValidationResult(isValid: false);
            var service = new Mock<IAggieEnterpriseService>(MockBehavior.Strict);
            if (useRechargeValidation)
            {
                service.Setup(item => item.IsRechargeAccountValid(GlChartString, RechargeAccount.CreditDebit.Debit, true))
                    .ReturnsAsync(expected);
            }
            else
            {
                service.Setup(item => item.IsAccountValid(GlChartString, true)).ReturnsAsync(expected);
            }
            var controller = CreateController(service.Object);

            var result = await controller.Validate(CreateRequest("Debit", useRechargeValidation));

            Assert.Null(result.Result);
            AssertValidationResult(expected, result.Value);
            service.VerifyAll();
        }

        [Theory]
        [InlineData("3-1234567", true)]
        [InlineData("3-1234567", false)]
        [InlineData("malformed-chart-string", true)]
        [InlineData("malformed-chart-string", false)]
        public async Task UnsupportedChartStringsReturnInvalidResultsWithoutCallingAggieEnterprise(
            string chartString, bool useRechargeValidation)
        {
            var service = new Mock<IAggieEnterpriseService>(MockBehavior.Strict);
            var controller = CreateController(service.Object);
            var request = CreateRequest("Credit", useRechargeValidation);
            request.ChartString = chartString;

            var result = await controller.Validate(request);

            var validation = Assert.IsType<ChartStringValidationResult>(result.Value);
            Assert.Null(result.Result);
            Assert.False(validation.IsValid);
            Assert.Equal(chartString, validation.ChartString);
            Assert.Contains("Only Aggie Enterprise GL and PPM chart strings are supported.", validation.Messages);
            Assert.Equal(string.Join(" ", validation.Messages), validation.Message);
            Assert.Empty(validation.Warnings);
            service.VerifyNoOtherCalls();
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public async Task JsonResponseContainsOnlyTheFivePublicFieldsWithCamelCaseNames(bool emptyMessagesAndWarnings)
        {
            var expected = CreateValidationResult(isValid: false);
            if (emptyMessagesAndWarnings)
            {
                expected.Messages.Clear();
                expected.Warnings.Clear();
            }
            var service = new Mock<IAggieEnterpriseService>(MockBehavior.Strict);
            service.Setup(item => item.IsRechargeAccountValid(GlChartString, RechargeAccount.CreditDebit.Credit, true))
                .ReturnsAsync(expected);
            var controller = CreateController(service.Object);
            var result = await controller.Validate(CreateRequest("Credit", useRechargeValidation: true));
            var services = new ServiceCollection();
            services.AddLogging();
            services.AddSingleton(Mock.Of<IWebHostEnvironment>());
            services.AddControllers().AddNewtonsoftJson();
            using var provider = services.BuildServiceProvider();
            var settings = provider.GetRequiredService<IOptions<MvcNewtonsoftJsonOptions>>().Value.SerializerSettings;

            var json = JObject.Parse(JsonConvert.SerializeObject(result.Value, settings));

            Assert.Equal(new[] { "chartString", "isValid", "message", "messages", "warnings" },
                json.Properties().Select(property => property.Name).OrderBy(name => name, StringComparer.Ordinal));
            Assert.Equal(expected.IsValid, json.Value<bool>("isValid"));
            Assert.Equal(expected.ChartString, json.Value<string>("chartString"));
            Assert.Equal(string.Join(" ", expected.Messages), json.Value<string>("message"));
            var messages = Assert.IsType<JArray>(json["messages"]);
            Assert.Equal(expected.Messages, messages.Values<string>());
            var warnings = Assert.IsType<JArray>(json["warnings"]);
            Assert.Equal(expected.Warnings.Count, warnings.Count);
            for (var index = 0; index < warnings.Count; index++)
            {
                var warning = Assert.IsType<JObject>(warnings[index]);
                Assert.Equal(new[] { "key", "value" },
                    warning.Properties().Select(property => property.Name).OrderBy(name => name, StringComparer.Ordinal));
                Assert.Equal(expected.Warnings[index].Key, warning.Value<string>("key"));
                Assert.Equal(expected.Warnings[index].Value, warning.Value<string>("value"));
            }

            var sharedModelJson = JObject.Parse(JsonConvert.SerializeObject(expected, settings));
            Assert.Equal(expected.AccountManager, sharedModelJson.Value<string>("accountManager"));
            Assert.NotEmpty(Assert.IsType<JArray>(sharedModelJson["details"]));
            Assert.NotEmpty(Assert.IsType<JArray>(sharedModelJson["approvers"]));
            Assert.NotNull(sharedModelJson["coaChartType"]);
        }

        [Theory]
        [InlineData(null, "Credit", true)]
        [InlineData("", "Credit", true)]
        [InlineData(" ", "Credit", true)]
        [InlineData(GlChartString, null, true)]
        [InlineData(GlChartString, "", true)]
        [InlineData(GlChartString, " ", true)]
        [InlineData(GlChartString, "invalid", true)]
        [InlineData(GlChartString, "0", true)]
        [InlineData(GlChartString, "1", true)]
        [InlineData(GlChartString, "2", true)]
        [InlineData(GlChartString, "Credit, Debit", true)]
        [InlineData(GlChartString, " Credit ", true)]
        [InlineData(GlChartString, "Credit", null)]
        [InlineData(GlChartString, null, false)]
        [InlineData(GlChartString, "0", false)]
        public async Task InvalidRequestFieldsReturnBadRequestWithoutCallingAggieEnterprise(
            string chartString, string direction, bool? useRechargeValidation)
        {
            var service = new Mock<IAggieEnterpriseService>(MockBehavior.Strict);
            var controller = CreateController(service.Object);
            var request = new ValidateChartStringRequest
            {
                ChartString = chartString,
                Direction = direction,
                UseRechargeValidation = useRechargeValidation
            };

            var result = await controller.Validate(request);

            Assert.IsType<BadRequestObjectResult>(result.Result);
            service.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task MissingBodyReturnsBadRequestWithoutCallingAggieEnterprise()
        {
            var service = new Mock<IAggieEnterpriseService>(MockBehavior.Strict);
            var controller = CreateController(service.Object);

            var result = await controller.Validate(null);

            Assert.IsType<BadRequestObjectResult>(result.Result);
            service.VerifyNoOtherCalls();
        }

        [Fact]
        public async Task ModelBindingErrorsReturnBadRequestWithoutCallingAggieEnterprise()
        {
            var service = new Mock<IAggieEnterpriseService>(MockBehavior.Strict);
            var controller = CreateController(service.Object);
            controller.ModelState.AddModelError(nameof(ValidateChartStringRequest.UseRechargeValidation), "Invalid boolean value.");

            var result = await controller.Validate(CreateRequest("Credit", useRechargeValidation: true));

            Assert.IsType<BadRequestObjectResult>(result.Result);
            service.VerifyNoOtherCalls();
        }

        [Fact]
        public void SwaggerDocumentsTheValidationRequestResultAndApiKeySecurity()
        {
            var securityScheme = new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "ApiKey" },
                Type = SecuritySchemeType.ApiKey,
                Name = ApiKeyMiddleware.HeaderKey,
                In = ParameterLocation.Header,
                Scheme = "ApiKey"
            };
            var services = new ServiceCollection();
            services.AddLogging();
            services.AddSingleton(Mock.Of<IWebHostEnvironment>());
            services.AddControllers().AddApplicationPart(typeof(ChartStringsApiController).Assembly);
            services.AddSwaggerGen(options =>
            {
                options.SwaggerDoc("v1", new OpenApiInfo { Title = "Payments API", Version = "v1" });
                options.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, "Payments.Mvc.xml"));
                options.AddSecurityDefinition("ApiKey", securityScheme);
                options.OperationFilter<SecurityRequirementsOperationFilter>(securityScheme);
            });
            using var provider = services.BuildServiceProvider();

            var swagger = provider.GetRequiredService<ISwaggerProvider>().GetSwagger("v1");

            var operation = swagger.Paths["/api/chartstrings/validate"].Operations[OperationType.Post];
            Assert.Contains("GL or PPM chart string", operation.Summary);
            Assert.Contains("Authorization header", operation.Description);
            Assert.Contains("775000", operation.Description);
            Assert.Contains("770006", operation.Description);
            Assert.Contains("Payments fund and revenue-account restrictions", operation.Description);
            Assert.Contains("Direction is required but does not affect validation", operation.Description);
            Assert.Contains("KFS accounts are not supported", operation.Description);
            Assert.Empty(operation.Parameters);
            Assert.Contains("200", operation.Responses.Keys);
            Assert.Contains("400", operation.Responses.Keys);
            Assert.Contains("401", operation.Responses.Keys);
            Assert.Contains("403", operation.Responses.Keys);
            var security = Assert.Single(Assert.Single(operation.Security));
            Assert.Equal("ApiKey", security.Key.Reference.Id);
            Assert.Contains(PolicyCodes.ApiKey, security.Value);
            Assert.Equal(ApiKeyMiddleware.HeaderKey, swagger.Components.SecuritySchemes["ApiKey"].Name);
            Assert.Equal(ParameterLocation.Header, swagger.Components.SecuritySchemes["ApiKey"].In);

            var request = operation.RequestBody.Content["application/json"].Schema;
            var requestSchema = swagger.Components.Schemas[request.Reference.Id];
            Assert.Equal(new[] { "chartString", "direction", "useRechargeValidation" },
                requestSchema.Properties.Keys.OrderBy(key => key, StringComparer.Ordinal));
            Assert.Equal(new[] { "chartString", "direction", "useRechargeValidation" },
                requestSchema.Required.OrderBy(key => key, StringComparer.Ordinal));
            Assert.Equal("string", requestSchema.Properties["chartString"].Type);
            Assert.Equal("string", requestSchema.Properties["direction"].Type);
            var directionPattern = requestSchema.Properties["direction"].Pattern;
            Assert.Matches(directionPattern, "Credit");
            Assert.Matches(directionPattern, "Debit");
            Assert.Matches(directionPattern, "cReDiT");
            Assert.Matches(directionPattern, "dEbIt");
            Assert.DoesNotMatch(directionPattern, "0");
            Assert.DoesNotMatch(directionPattern, "invalid");
            Assert.Contains("Credit or Debit", requestSchema.Properties["direction"].Description);
            Assert.Equal("boolean", requestSchema.Properties["useRechargeValidation"].Type);
            Assert.Contains("recharge validation", requestSchema.Properties["useRechargeValidation"].Description);

            var response = operation.Responses["200"].Content["application/json"].Schema;
            Assert.Equal(nameof(ChartStringValidationResult), response.Reference.Id);
            var responseSchema = swagger.Components.Schemas[response.Reference.Id];
            Assert.Equal(new[] { "chartString", "isValid", "message", "messages", "warnings" },
                responseSchema.Properties.Keys.OrderBy(key => key, StringComparer.Ordinal));
            Assert.Equal("boolean", responseSchema.Properties["isValid"].Type);
            Assert.Equal("string", responseSchema.Properties["chartString"].Type);
            Assert.Equal("string", responseSchema.Properties["message"].Type);
            Assert.Equal("array", responseSchema.Properties["messages"].Type);
            Assert.Equal("string", responseSchema.Properties["messages"].Items.Type);
            Assert.Equal("array", responseSchema.Properties["warnings"].Type);
        }

        private static void AssertValidationResult(AccountValidationModel expected, ChartStringValidationResult actual)
        {
            Assert.NotNull(actual);
            Assert.Equal(expected.IsValid, actual.IsValid);
            Assert.Equal(expected.ChartString, actual.ChartString);
            Assert.Equal(expected.Warnings, actual.Warnings);
            Assert.Equal(expected.Message, actual.Message);
            Assert.Equal(expected.Messages, actual.Messages);
        }

        private static AccountValidationModel CreateValidationResult(bool isValid)
        {
            return new AccountValidationModel
            {
                IsValid = isValid,
                ChartString = "normalized-chart-string",
                CoaChartType = FinancialChartStringType.Gl,
                Messages = new List<string> { "First validation message.", "Second validation message." },
                Warnings = new List<KeyValuePair<string, string>> { new KeyValuePair<string, string>("Natural account", "Account was normalized.") },
                Details = new List<KeyValuePair<string, string>> { new KeyValuePair<string, string>("Fund", "Synthetic test fund.") },
                AccountManager = "Synthetic Manager",
                AccountManagerEmail = "manager@example.com",
                Approvers = new List<Approver> { new Approver { FullName = "Synthetic Approver", Email = "approver@example.com" } }
            };
        }

        private static ValidateChartStringRequest CreateRequest(
            string direction, bool useRechargeValidation, string chartString = GlChartString)
        {
            return new ValidateChartStringRequest
            {
                ChartString = chartString,
                Direction = direction,
                UseRechargeValidation = useRechargeValidation
            };
        }

        private static ChartStringsApiController CreateController(IAggieEnterpriseService service)
        {
            var dbContext = new Mock<ApplicationDbContext>(new DbContextOptions<ApplicationDbContext>());
            return new ChartStringsApiController(dbContext.Object, service);
        }
    }
}
