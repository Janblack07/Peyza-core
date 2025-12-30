using Volo.Abp.Modularity;

namespace Peyza.Core.NotificationManagement;

/* Inherit from this class for your domain layer tests.
 * See SampleManager_Tests for example.
 */
public abstract class NotificationManagementDomainTestBase<TStartupModule> : NotificationManagementTestBase<TStartupModule>
    where TStartupModule : IAbpModule
{

}
