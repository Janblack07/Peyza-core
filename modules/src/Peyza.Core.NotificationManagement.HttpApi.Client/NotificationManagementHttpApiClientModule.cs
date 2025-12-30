using Microsoft.Extensions.DependencyInjection;
using Volo.Abp.Http.Client;
using Volo.Abp.Modularity;
using Volo.Abp.VirtualFileSystem;

namespace Peyza.Core.NotificationManagement;

[DependsOn(
    typeof(NotificationManagementApplicationContractsModule),
    typeof(AbpHttpClientModule))]
public class NotificationManagementHttpApiClientModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        context.Services.AddHttpClientProxies(
            typeof(NotificationManagementApplicationContractsModule).Assembly,
            NotificationManagementRemoteServiceConsts.RemoteServiceName
        );

        Configure<AbpVirtualFileSystemOptions>(options =>
        {
            options.FileSets.AddEmbedded<NotificationManagementHttpApiClientModule>();
        });

    }
}
