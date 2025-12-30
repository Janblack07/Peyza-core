using Volo.Abp.Modularity;

namespace Peyza.Core.NotificationManagement;

[DependsOn(
    typeof(NotificationManagementApplicationModule),
    typeof(NotificationManagementDomainTestModule)
    )]
public class NotificationManagementApplicationTestModule : AbpModule
{

}
