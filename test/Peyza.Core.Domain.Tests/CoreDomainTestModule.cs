using Volo.Abp.Modularity;

namespace Peyza.Core;

[DependsOn(
    typeof(CoreDomainModule),
    typeof(CoreTestBaseModule)
)]
public class CoreDomainTestModule : AbpModule
{

}
