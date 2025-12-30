using System;
using System.Collections.Generic;
using System.Text;

namespace Peyza.Core.NotificationManagement.Providers.SendGrid
{
    public class SendGridOptions
    {
        public string ApiKey { get; set; } = default!;
        public string FromEmail { get; set; } = default!;
        public string? FromName { get; set; }
    }
}
