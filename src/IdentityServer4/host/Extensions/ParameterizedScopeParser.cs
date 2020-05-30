using IdentityServer4.Validation;
using System;

namespace Host.Extensions
{
    public class ParameterizedScopeParser : DefaultScopeParser
    {
        public override ParseScopeResult ParseScopeValue(string scopeValue)
        {
            const string transactionScopeName = "transaction";
            const string separator = ":";
            const string transactionScopePrefix = transactionScopeName + separator;

            if (scopeValue.StartsWith(transactionScopePrefix))
            {
                var parts = scopeValue.Split(separator, StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length == 2)
                {
                    return new ParseScopeResult(new ParsedScopeValue(scopeValue, transactionScopeName, parts[1]));
                }

                return new ParseScopeResult(new ParsedScopeValidationError(scopeValue, "transaction scope missing transaction parameter value"));
            }

            return base.ParseScopeValue(scopeValue);
        }
    }
}
