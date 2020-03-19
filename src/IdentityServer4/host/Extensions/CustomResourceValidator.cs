using IdentityServer4.Stores;
using IdentityServer4.Validation;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Host.Extensions
{
    public class CustomResourceValidator : ResourceValidator
    {
        public CustomResourceValidator(IResourceStore store, ILogger<ResourceValidator> logger) : base(store, logger)
        {
        }

        public override async Task<IEnumerable<ParsedScopeValue>> ParseRequestedScopes(IEnumerable<string> scopeValues)
        {
            const string transactionPrefix = "transaction:";

            var transaction = scopeValues.Where(x => x.StartsWith(transactionPrefix)).FirstOrDefault();
            if (transaction != null)
            {
                scopeValues = scopeValues.Except(new[] { transaction });
            }

            var result = await base.ParseRequestedScopes(scopeValues);
            
            if (transaction != null)
            {
                var list = result.ToList();
                list.Add(new ParsedScopeValue("transaction", transaction));
                result = list.AsEnumerable();
            }

            return result;
        }
    }
}
