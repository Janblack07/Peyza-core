using Volo.Abp.Modularity;

namespace Peyza.Core.NotificationManagement;

/* Inherit from this class for your application layer tests.
 * See SampleAppService_Tests for example.
 */
public abstract class NotificationManagementApplicationTestBase<TStartupModule> : NotificationManagementTestBase<TStartupModule>
    where TStartupModule : IAbpModule
{

}
