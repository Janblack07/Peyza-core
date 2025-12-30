using System;
using System.Collections.Generic;
using System.Text;

namespace Peyza.Core.NotificationManagement.Dtos
{
    public class NotificationMessageSummaryDto
    {
        public Guid Id { get; set; }
        public NotificationChannel Channel { get; set; }
        public string Destination { get; set; } = default!;
        public string? Subject { get; set; }
        public NotificationStatus Status { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? ScheduledAt { get; set; }
        public DateTime? SentAt { get; set; }
    }
}
