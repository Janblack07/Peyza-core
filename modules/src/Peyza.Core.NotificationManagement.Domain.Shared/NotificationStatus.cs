using System;
using System.Collections.Generic;
using System.Text;

namespace Peyza.Core.NotificationManagement
{
    public enum NotificationStatus
    {
        Pending = 0,
        Scheduled = 1,
        Sending = 2,
        Sent = 3,
        Failed = 4,
        Canceled = 5
    }
}
