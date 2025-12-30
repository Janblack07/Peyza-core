using Peyza.Core.Localization;
using Volo.Abp.Application.Services;

namespace Peyza.Core;

public abstract class CoreAppService : ApplicationService
{
    protected CoreAppService()
    {
        LocalizationResource = typeof(CoreResource);
        ObjectMapperContext = typeof(CoreApplicationModule);
    }
}
