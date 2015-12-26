using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNet.Http;
using Microsoft.Extensions.Logging;

namespace IdentityServer4.Core.Results
{
    public class PipelineResult<TModel> : IResult, IPipelineResult
    {
        private readonly PathString _path;

        public TModel Model { get; set; }

        public PipelineResult(string path)
        {
            if (path != "" && !path.StartsWith("/"))
            {
                path = "/" + path;
            }

            _path = new PathString(path);
        }

        public PipelineResult(string path, TModel model)
            : this(path)
        {
            Model = model;
        }

        public Task ExecuteAsync(HttpContext context, ILogger logger)
        {
            logger.LogVerbose("Changing request path to: {0}", _path.ToString());
            context.Request.Path = _path;

            EnsureModel();
            if (Model != null)
            {
                context.Items["idsvr.pipelineresult.model." + Model.GetType()] = Model;
            }

            return Task.FromResult(0);
        }

        protected virtual TModel GetModel()
        {
            return Model;
        }

        void EnsureModel()
        {
            if (Model == null)
            {
                Model = GetModel();
            }
        }
    }
}
