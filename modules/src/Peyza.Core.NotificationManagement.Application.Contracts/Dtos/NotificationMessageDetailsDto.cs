using System;
using System.Collections.Generic;
using System.Text;

namespace Peyza.Core.NotificationManagement.Dtos
{
    public class NotificationMessageDetailsDto
    {
        public Guid Id { get; set; }

        public Guid? DebtorId { get; set; }
        public Guid? DebtId { get; set; }
        public Guid? PaymentOrderId { get; set; }
        public Guid? TemplateId { get; set; }

        public NotificationChannel Channel { get; set; }
        public string Destination { get; set; } = default!;
        public string? Subject { get; set; }

        public string Body { get; set; } = default!;

        public NotificationStatus Status { get; set; }

        public DateTime CreatedAt { get; set; }
        public DateTime? ScheduledAt { get; set; }
        public DateTime? SentAt { get; set; }

        public string? ProviderMessageId { get; set; }
        public string? ErrorCode { get; set; }
    }
}
