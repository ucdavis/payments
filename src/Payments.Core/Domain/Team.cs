using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;

namespace Payments.Core.Domain
{
    //TODO: Add flag for recharges
    public class Team
    {
        public Team()
        {
            Accounts = new List<FinancialAccount>();
            Coupons = new List<Coupon>();
            Permissions = new List<TeamPermission>();
            WebHooks = new List<WebHook>();
        }

        [Key]
        public int Id { get; set; }

        [StringLength(128)]
        [Display(Name = "Team Name")]
        [Required]
        public string Name { get; set; }

        [Display(Name = "Team Slug")]
        [Required]
        [StringLength(40, MinimumLength = 3, ErrorMessage = "Slug must be between 3 and 40 characters")]
        [RegularExpression(SlugRegex,
            ErrorMessage = "Slug may only contain lowercase alphanumeric characters or single hyphens, and cannot begin or end with a hyphen")]
        public string Slug { get; set; }

        public const string SlugRegex = "^([a-z0-9]+[a-z0-9\\-]?)+[a-z0-9]$";

        [StringLength(128)]
        [Display(Name = "Contact Name")]
        public string ContactName { get; set; }

        [EmailAddress]
        [StringLength(128)]
        [Display(Name = "Contact Email")]
        public string ContactEmail { get; set; }

        [Phone]
        [StringLength(40)]
        [Display(Name = "Contact Phone Number")]
        public string ContactPhoneNumber { get; set; }

        [Display(Name = "Active")]
        public bool IsActive { get; set; } = true;

        [JsonIgnore]
        public IList<FinancialAccount> Accounts { get; set; }

        [JsonIgnore]
        public IList<Coupon> Coupons { get; set; }

        [JsonIgnore]
        public IList<TeamPermission> Permissions { get; set; }

        [JsonIgnore]
        public IList<WebHook> WebHooks { get; set; }

        [JsonIgnore]
        public string ApiKey { get; set; }

        [JsonIgnore]
        [StringLength(128)]
        public string WebHookApiKey { get; set; }

        [Required]
        [StringLength(10)]
        public string AllowedInvoiceType { get; set; } = AllowedInvoiceTypes.CreditCard;

        [NotMapped]
        public FinancialAccount DefaultAccount {
            get {
                return Accounts.FirstOrDefault(a => a.IsDefault);
            }
        }

        public TeamPermission AddPermission(User user, TeamRole role)
        {
            var permission = new TeamPermission()
            {
                Team = this,
                User = user,
                Role = role,
            };

            Permissions.Add(permission);

            return permission;
        }

        protected internal static void OnModelCreating(ModelBuilder builder)
        {
            builder.Entity<Team>()
                .Property(t => t.AllowedInvoiceType)
                .HasMaxLength(10)
                .HasDefaultValue(AllowedInvoiceTypes.CreditCard);
        }

        /// <summary>
        /// These are a superset of the Invoice.Types
        /// (Adds Both value)
        /// </summary>
        public static class AllowedInvoiceTypes
        {
            public const string CreditCard = "CC";
            public const string Recharge = "Recharge";
            public const string Both = "Both";
        }
    }
}
