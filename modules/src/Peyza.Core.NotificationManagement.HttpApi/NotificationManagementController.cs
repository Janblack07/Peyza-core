using Peyza.Core.NotificationManagement.Localization;
using Volo.Abp.AspNetCore.Mvc;

namespace Peyza.Core.NotificationManagement;

public abstract class NotificationManagementController : AbpControllerBase
{
    protected NotificationManagementController()
    {
        LocalizationResource = typeof(NotificationManagementResource);
    }
}
