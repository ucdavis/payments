﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Transactions;
using Xunit.Sdk;

namespace Payments.Tests.Helpers
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
    public sealed class AutoRollbackAttribute : BeforeAfterTestAttribute
    {
        TransactionScope scope;

        /// <summary>
        /// Gets or sets whether transaction flow across thread continuations is enabled for TransactionScope.
        /// By default transaction flow across thread continuations is enabled.
        /// </summary>
        public TransactionScopeAsyncFlowOption AsyncFlowOption { get; set; } = TransactionScopeAsyncFlowOption.Enabled;

        /// <summary>
        /// Gets or sets the isolation level of the transaction.
        /// Default value is <see cref="IsolationLevel"/>.Unspecified.
        /// </summary>
        public IsolationLevel IsolationLevel { get; set; } = IsolationLevel.Unspecified;

        /// <summary>
        /// Gets or sets the scope option for the transaction.
        /// Default value is <see cref="TransactionScopeOption"/>.Required.
        /// </summary>
        public TransactionScopeOption ScopeOption { get; set; } = TransactionScopeOption.Required;

        /// <summary>
        /// Gets or sets the timeout of the transaction, in milliseconds.
        /// By default, the transaction will not timeout.
        /// </summary>
        public long TimeoutInMS { get; set; } = -1;

        /// <summary>
        /// Rolls back the transaction.
        /// </summary>
        public override void After(MethodInfo methodUnderTest)
        {
            scope.Dispose();
        }

        /// <summary>
        /// Creates the transaction.
        /// </summary>
        public override void Before(MethodInfo methodUnderTest)
        {
            var options = new TransactionOptions { IsolationLevel = IsolationLevel };
            if (TimeoutInMS > 0)
                options.Timeout = TimeSpan.FromMilliseconds(TimeoutInMS);

            scope = new TransactionScope(ScopeOption, options, AsyncFlowOption);
        }
    }
}
