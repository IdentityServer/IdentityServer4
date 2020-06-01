using IdentityServer4.Validation;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Host.Extensions
{
    //public class Test : DefaultScopeParser, IScopeParser
    //{
    //    public ParsedScopesResult ParseScopeValues(IEnumerable<string> scopeValues)
    //    {
    //        var txScope = scopeValues.FirstOrDefault(x => x.StartsWith("transaction:"));

    //        if (txScope != null)
    //        {
    //            scopeValues = scopeValues.Except(txScope);

    //            // do the real tx work here...

    //        }

    //        var result = base.ParseScopeValues(scopeValues);


    //        if (txScope != null)
    //        {
    //            // do the real tx work here... and add to result
    //            result.ParsedScope.a


    //        }
    //    }
    //}

    public class ParameterizedScopeParser : DefaultScopeParser
    {
        public override ParseScopeResult ParseScopeValue(string scopeValue)
        {
            const string transactionScopeName = "transaction";
            const string separator = ":";
            const string transactionScopePrefix = transactionScopeName + separator;

            if (scopeValue.StartsWith(transactionScopePrefix))
            {
                // we get in here with a scope like "transaction:something"
                var parts = scopeValue.Split(separator, StringSplitOptions.RemoveEmptyEntries);
                if (parts.Length == 2)
                {
                    return new ParseScopeResult(new ParsedScopeValue(scopeValue, transactionScopeName, parts[1]));
                }

                return new ParseScopeResult(new ParsedScopeValidationError(scopeValue, "transaction scope missing transaction parameter value"));
            }
            else if (scopeValue != transactionScopeName)
            {
                // we get in here with a scope not like "transaction"
                return base.ParseScopeValue(scopeValue);
            }
            else
            {
                // we get in here with a scope exactly "transaction", which is to say we're ignoring it 
                // and not including it in the results
                return null;
            }
        }
    }
}
