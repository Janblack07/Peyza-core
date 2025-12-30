using Volo.Abp.Domain;
using Volo.Abp.Modularity;

namespace Peyza.Core.NotificationManagement;

[DependsOn(
    typeof(AbpDddDomainModule),
    typeof(NotificationManagementDomainSharedModule)
)]
public class NotificationManagementDomainModule : AbpModule
{

}
