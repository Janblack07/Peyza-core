using Peyza.Core.NotificationManagement.Dtos;
using Peyza.Core.NotificationManagement.Providers;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Entities;
using Volo.Abp.Domain.Repositories;

namespace Peyza.Core.NotificationManagement.Handlers
{
    public class SendNotificationHandler : ITransientDependency
    {
        private readonly IRepository<MessageTemplate, Guid> _templateRepo;
        private readonly IRepository<NotificationMessage, Guid> _messageRepo;
        private readonly INotificationProviderDispatcher _dispatcher;

        public SendNotificationHandler(
            IRepository<MessageTemplate, Guid> templateRepo,
            IRepository<NotificationMessage, Guid> messageRepo,
            INotificationProviderDispatcher dispatcher)
        {
            _templateRepo = templateRepo;
            _messageRepo = messageRepo;
            _dispatcher = dispatcher;
        }

        public async Task<NotificationMessageResponseDto> HandleAsync(
            NotificationMessageRequestDto input,
            CancellationToken cancellationToken = default)
        {
            // 1) Resolver plantilla (si aplica)
            MessageTemplate? template = null;
            if (input.TemplateId.HasValue)
            {
                template = await _templateRepo.FindAsync(input.TemplateId.Value, cancellationToken: cancellationToken);
                if (template is null)
                {
                    // Mantén simple: si no existe, no se envía
                    // (Luego lo elevamos a BusinessException con código estable)
                    throw new EntityNotFoundException(typeof(MessageTemplate), input.TemplateId.Value);
                }
            }

            var finalSubject = input.Subject ?? template?.SubjectTemplate;
            var finalBody = string.IsNullOrWhiteSpace(input.Body) ? template?.BodyTemplate : input.Body;

            if (string.IsNullOrWhiteSpace(finalBody))
                throw new Exception("BODY_REQUIRED");

            // 2) Crear entidad dominio
            var id = Guid.NewGuid();
            var msg = new NotificationMessage(
                id,
                input.Channel,
                input.Destination,
                finalBody!,
                finalSubject,
                input.DebtorId,
                input.DebtId,
                input.PaymentOrderId,
                input.TemplateId
            );

            // 3) Si es programado => guardar y salir
            if (input.ScheduledAt.HasValue)
            {
                msg.Schedule(input.ScheduledAt.Value);
                await _messageRepo.InsertAsync(msg, autoSave: true, cancellationToken: cancellationToken);

                return new NotificationMessageResponseDto
                {
                    Id = msg.Id,
                    Status = msg.Status,
                    CreatedAt = msg.CreatedAt
                };
            }

            // 4) Envío inmediato (Pending -> Sending -> Sent/Failed)
            msg.MarkAsSending();
            await _messageRepo.InsertAsync(msg, autoSave: true, cancellationToken: cancellationToken);

            var result = await _dispatcher.SendAsync(msg, cancellationToken);

            if (result.Success && !string.IsNullOrWhiteSpace(result.ProviderMessageId))
            {
                msg.MarkAsSent(result.ProviderMessageId);
            }
            else
            {
                msg.MarkAsFailed(result.ErrorCode ?? "PROVIDER_FAILED");
            }

            await _messageRepo.UpdateAsync(msg, autoSave: true, cancellationToken: cancellationToken);

            return new NotificationMessageResponseDto
            {
                Id = msg.Id,
                Status = msg.Status,
                CreatedAt = msg.CreatedAt
            };
        }
    }
}
