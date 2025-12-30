using Peyza.Core.Localization;
using Volo.Abp.AspNetCore.Mvc;

namespace Peyza.Core;

public abstract class CoreController : AbpControllerBase
{
    protected CoreController()
    {
        LocalizationResource = typeof(CoreResource);
    }
}
