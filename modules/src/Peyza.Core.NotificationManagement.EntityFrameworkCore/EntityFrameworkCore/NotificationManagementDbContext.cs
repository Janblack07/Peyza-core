using Microsoft.EntityFrameworkCore;
using Volo.Abp.Data;
using Volo.Abp.EntityFrameworkCore;

namespace Peyza.Core.NotificationManagement.EntityFrameworkCore;

[ConnectionStringName(NotificationManagementDbProperties.ConnectionStringName)]
public class NotificationManagementDbContext : AbpDbContext<NotificationManagementDbContext>
{
    public DbSet<NotificationMessage> NotificationMessages => Set<NotificationMessage>();
    public DbSet<MessageTemplate> MessageTemplates => Set<MessageTemplate>();

    public NotificationManagementDbContext(DbContextOptions<NotificationManagementDbContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<NotificationMessage>(b =>
        {
            b.ToTable("NotificationMessages", "notification");
            b.HasKey(x => x.Id);

            b.Property(x => x.Destination).HasMaxLength(200).IsRequired();
            b.Property(x => x.Subject).HasMaxLength(200);
            b.Property(x => x.ProviderMessageId).HasMaxLength(200);
            b.Property(x => x.ErrorCode).HasMaxLength(50);

            b.Property(x => x.Body).IsRequired();

            b.HasIndex(x => x.CreatedAt);
            b.HasIndex(x => x.Status);
        });

        builder.Entity<MessageTemplate>(b =>
        {
            b.ToTable("MessageTemplates", "notification");
            b.HasKey(x => x.Id);

            b.Property(x => x.Name).HasMaxLength(100).IsRequired();
            b.Property(x => x.Code).HasMaxLength(100);
            b.Property(x => x.SubjectTemplate).HasMaxLength(200);
            b.Property(x => x.BodyTemplate).IsRequired();

            b.HasIndex(x => new { x.Name, x.Channel }).IsUnique();
        });
    }
}
