using System.Threading.Tasks;
using Microsoft.AspNet.Http;
using IdentityServer4.Core.Hosting;

namespace IdentityServer4.Core.Results
{
    public class PipelineResult<TModel> : IEndpointResult, IPipelineResult
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

        public Task ExecuteAsync(IdentityServerContext context)
        {
            context.HttpContext.Request.Path = _path;

            EnsureModel();
            if (Model != null)
            {
                context.HttpContext.Items["idsvr.pipelineresult.model." + Model.GetType()] = Model;
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
