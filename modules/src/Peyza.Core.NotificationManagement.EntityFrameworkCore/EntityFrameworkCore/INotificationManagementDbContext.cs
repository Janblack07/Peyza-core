using Volo.Abp.Data;
using Volo.Abp.EntityFrameworkCore;

namespace Peyza.Core.NotificationManagement.EntityFrameworkCore;

[ConnectionStringName(NotificationManagementDbProperties.ConnectionStringName)]
public interface INotificationManagementDbContext : IEfCoreDbContext
{
    /* Add DbSet for each Aggregate Root here. Example:
     * DbSet<Question> Questions { get; }
     */
}
