using Volo.Abp.Reflection;

namespace Peyza.Core.NotificationManagement.Permissions;

public class NotificationManagementPermissions
{
    public const string GroupName = "NotificationManagement";

    public static string[] GetAll()
    {
        return ReflectionHelper.GetPublicConstantsRecursively(typeof(NotificationManagementPermissions));
    }
}
