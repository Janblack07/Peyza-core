namespace Peyza.Core.NotificationManagement;

public static class NotificationManagementDbProperties
{
    public static string DbTablePrefix { get; set; } = "NotificationManagement";

    public static string? DbSchema { get; set; } = null;

    public const string ConnectionStringName = "NotificationManagement";
}
