using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNet.Http;
using Microsoft.Extensions.Logging;

namespace IdentityServer4.Core.Results
{
    public class ErrorPageResult : PipelineResult
    {
        public ErrorPageResult(string errorMessage)
            : base(Constants.RoutePaths.Error, errorMessage)
        {
        }
    }
}
