using System;
using System.Collections.Generic;
using System.Text;

namespace Peyza.Core.NotificationManagement.Dtos
{
    public class NotificationMessageRequestDto
    {
        public Guid? DebtorId { get; set; }
        public Guid? DebtId { get; set; }
        public Guid? PaymentOrderId { get; set; }
        public Guid? TemplateId { get; set; }

        public NotificationChannel Channel { get; set; }
        public string Destination { get; set; } = default!;
        public string? Subject { get; set; }
        public string Body { get; set; } = default!;
        public DateTime? ScheduledAt { get; set; }
    }
}
