using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNet.Http;
using Microsoft.Extensions.Logging;

namespace IdentityServer4.Core.Results
{
    public class PipelineResult : IResult, IPipelineResult
    {
        private readonly PathString _path;

        public object Model { get; set; }

        public PipelineResult(string path)
        {
            if (path != "" && !path.StartsWith("/"))
            {
                path = "/" + path;
            }

            _path = new PathString(path);
        }

        public PipelineResult(string path, object model)
            : this(path)
        {
            Model = model;
        }

        public Task ExecuteAsync(HttpContext context, ILogger logger)
        {
            logger.LogVerbose("Changing request path to: {0}", _path.ToString());
            context.Request.Path = _path;
            var model = GetModel();
            if (model != null)
            {
                context.Items["idsvr.pipelineresult.model." + model.GetType()] = model;
            }
            return Task.FromResult(0);
        }

        protected virtual object GetModel()
        {
            return Model;
        }
    }
}
