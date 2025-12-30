using Peyza.Core.NotificationManagement.Localization;
using Volo.Abp.Application.Services;

namespace Peyza.Core.NotificationManagement;

public abstract class NotificationManagementAppService : ApplicationService
{
    protected NotificationManagementAppService()
    {
        LocalizationResource = typeof(NotificationManagementResource);
        ObjectMapperContext = typeof(NotificationManagementApplicationModule);
    }
}
