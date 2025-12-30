using System;
using System.Threading;
using System.Threading.Tasks;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Timing;

namespace Peyza.Core.NotificationManagement.Handlers
{
    public class UpdateDeliveryStatusHandler : ITransientDependency
    {
        private readonly IRepository<NotificationMessage, Guid> _messageRepo;
        private readonly IClock _clock;

        public UpdateDeliveryStatusHandler(
            IRepository<NotificationMessage, Guid> messageRepo,
            IClock clock)
        {
            _messageRepo = messageRepo;
            _clock = clock;
        }

        public async Task HandleAsync(
            Guid messageId,
            string eventType,
            string? providerMessageId,
            string? reason,
            CancellationToken ct = default)
        {
            var msg = await _messageRepo.FindAsync(messageId, cancellationToken: ct);
            if (msg is null) return;

            var e = eventType?.Trim().ToLowerInvariant();
            if (string.IsNullOrWhiteSpace(e)) return;

            // Idempotencia: si ya está Sent, ignora eventos repetidos.
            if (msg.Status == NotificationStatus.Sent)
                return;

            // Nota: con las invariantes nuevas, MarkAsSent solo es válido desde Sending.
            // El webhook puede llegar tarde o fuera de orden; manejamos con checks seguros.
            if (e is "delivered")
            {
                var pid = !string.IsNullOrWhiteSpace(providerMessageId)
                    ? providerMessageId!
                    : $"sendgrid:{msg.Id:N}";

                // Si aún no estaba en Sending, no forzamos excepción:
                // - Si está Scheduled/Pending: lo dejamos como está (el worker/handler hará el envío y lo marcará)
                // - Si está Failed/Canceled: no lo cambiamos por delivered (inconsistente)
                if (msg.Status == NotificationStatus.Sending)
                {
                    msg.MarkAsSent(pid, _clock.Now);
                    await _messageRepo.UpdateAsync(msg, autoSave: true, cancellationToken: ct);
                }

                return;
            }

            if (e is "bounce" or "dropped" or "spamreport" or "blocked" or "deferred")
            {
                // Solo marcamos failed si está Sending.
                // Si ya está Failed/Canceled/Scheduled, no forzamos.
                if (msg.Status == NotificationStatus.Sending)
                {
                    msg.MarkAsFailed($"SENDGRID_{e.ToUpperInvariant()}");
                    await _messageRepo.UpdateAsync(msg, autoSave: true, cancellationToken: ct);
                }

                return;
            }

            // open/click/processed/etc -> ignorar por ahora
        }
    }
}
