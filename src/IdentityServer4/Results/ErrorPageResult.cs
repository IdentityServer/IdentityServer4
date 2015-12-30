using IdentityServer4.Core.ViewModels;

namespace IdentityServer4.Core.Results
{
    public class ErrorPageResult : PipelineResult<ErrorViewModel>
    {
        public ErrorPageResult(ErrorViewModel model)
            : base(Constants.RoutePaths.Error, model)
        {
        }
    }
}
