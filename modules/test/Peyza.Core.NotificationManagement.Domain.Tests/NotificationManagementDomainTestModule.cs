using Volo.Abp.Modularity;

namespace Peyza.Core.NotificationManagement;

[DependsOn(
    typeof(NotificationManagementDomainModule),
    typeof(NotificationManagementTestBaseModule)
)]
public class NotificationManagementDomainTestModule : AbpModule
{

}
