using Volo.Abp.Modularity;
using Volo.Abp.VirtualFileSystem;

namespace Peyza.Core.NotificationManagement;

[DependsOn(
    typeof(AbpVirtualFileSystemModule)
    )]
public class NotificationManagementInstallerModule : AbpModule
{
    public override void ConfigureServices(ServiceConfigurationContext context)
    {
        Configure<AbpVirtualFileSystemOptions>(options =>
        {
            options.FileSets.AddEmbedded<NotificationManagementInstallerModule>();
        });
    }
}
