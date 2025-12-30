using System;
using Volo.Abp;
using Volo.Abp.Domain.Entities;

namespace Peyza.Core.NotificationManagement
{
    public class NotificationMessage : AggregateRoot<Guid>
    {
        public Guid? DebtorId { get; private set; }
        public Guid? DebtId { get; private set; }
        public Guid? PaymentOrderId { get; private set; }
        public Guid? TemplateId { get; private set; }

        public NotificationChannel Channel { get; private set; }
        public string Destination { get; private set; } = default!;
        public string? Subject { get; private set; }
        public string Body { get; private set; } = default!;

        public NotificationStatus Status { get; private set; }

        public DateTime CreatedAt { get; private set; }
        public DateTime? ScheduledAt { get; private set; }
        public DateTime? SentAt { get; private set; }

        public string? ProviderMessageId { get; private set; }
        public string? ErrorCode { get; private set; }

        private NotificationMessage() { }

        public NotificationMessage(
            Guid id,
            NotificationChannel channel,
            string destination,
            string body,
            DateTime createdAtUtc,
            string? subject = null,
            Guid? debtorId = null,
            Guid? debtId = null,
            Guid? paymentOrderId = null,
            Guid? templateId = null
        ) : base(id)
        {
            Channel = channel;
            Destination = Check.NotNullOrWhiteSpace(destination, nameof(destination), maxLength: 200);
            Body = Check.NotNullOrWhiteSpace(body, nameof(body));
            Subject = subject is null ? null : Check.Length(subject, nameof(subject), maxLength: 200);

            DebtorId = debtorId;
            DebtId = debtId;
            PaymentOrderId = paymentOrderId;
            TemplateId = templateId;

            CreatedAt = createdAtUtc;
            Status = NotificationStatus.Pending;
        }

        public void Schedule(DateTime sendAtUtc)
        {
            if (Status != NotificationStatus.Pending)
                throw new BusinessException("NOTIFICATION_INVALID_STATE_TO_SCHEDULE");

            ScheduledAt = sendAtUtc;
            Status = NotificationStatus.Scheduled;
        }

        public void MarkAsSending()
        {
            if (Status is not (NotificationStatus.Pending or NotificationStatus.Scheduled))
                throw new BusinessException("NOTIFICATION_INVALID_STATE_TO_SENDING");

            Status = NotificationStatus.Sending;
        }

        public void MarkAsSent(string providerMessageId, DateTime sentAtUtc)
        {
            if (Status != NotificationStatus.Sending)
                throw new BusinessException("NOTIFICATION_INVALID_STATE_TO_SENT");

            ProviderMessageId = Check.Length(providerMessageId, nameof(providerMessageId), maxLength: 200);
            SentAt = sentAtUtc;
            Status = NotificationStatus.Sent;
            ErrorCode = null;
        }

        public void MarkAsFailed(string errorCode)
        {
            if (Status != NotificationStatus.Sending)
                throw new BusinessException("NOTIFICATION_INVALID_STATE_TO_FAILED");

            ErrorCode = Check.Length(errorCode, nameof(errorCode), maxLength: 50);
            Status = NotificationStatus.Failed;
        }

        public void Cancel()
        {
            if (Status is NotificationStatus.Sent)
                throw new BusinessException("NOTIFICATION_ALREADY_SENT");

            if (Status is NotificationStatus.Canceled)
                return; // idempotente

            if (Status is not (NotificationStatus.Pending or NotificationStatus.Scheduled))
                throw new BusinessException("NOTIFICATION_INVALID_STATE_TO_CANCEL");

            Status = NotificationStatus.Canceled;
        }

        public void Retry(DateTime scheduledAtUtc)
        {
            if (Status != NotificationStatus.Failed)
                throw new BusinessException("NOTIFICATION_RETRY_ONLY_FAILED");

            ErrorCode = null;
            ProviderMessageId = null;
            SentAt = null;

            ScheduledAt = scheduledAtUtc;
            Status = NotificationStatus.Scheduled;
        }
    }
}
