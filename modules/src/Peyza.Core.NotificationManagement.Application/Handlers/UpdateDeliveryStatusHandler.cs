using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Repositories;

namespace Peyza.Core.NotificationManagement.Handlers
{
    public class UpdateDeliveryStatusHandler : ITransientDependency
    {
        private readonly IRepository<NotificationMessage, Guid> _messageRepo;

        public UpdateDeliveryStatusHandler(IRepository<NotificationMessage, Guid> messageRepo)
        {
            _messageRepo = messageRepo;
        }

        public async Task HandleAsync(Guid messageId, string eventType, string? providerMessageId, string? reason, CancellationToken ct = default)
        {
            var msg = await _messageRepo.FindAsync(messageId, cancellationToken: ct);
            if (msg is null) return;

            // Reglas mínimas:
            // - delivered => Sent
            // - bounce/dropped/spamreport => Failed
            // - open/click => (por ahora no cambiamos)
            var e = eventType?.Trim().ToLowerInvariant();

            if (e is "delivered")
            {
                if (!string.IsNullOrWhiteSpace(providerMessageId))
                    msg.MarkAsSent(providerMessageId);
                else
                    msg.MarkAsSent($"sendgrid:{msg.Id:N}");
            }
            else if (e is "bounce" or "dropped" or "spamreport" or "blocked" or "deferred")
            {
                msg.MarkAsFailed($"SENDGRID_{e?.ToUpperInvariant()}");
            }
            else
            {
                // otros eventos: ignorar por ahora
                return;
            }

            await _messageRepo.UpdateAsync(msg, autoSave: true, cancellationToken: ct);
        }
    }
}
