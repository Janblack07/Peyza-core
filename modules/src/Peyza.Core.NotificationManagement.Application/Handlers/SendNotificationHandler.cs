using Peyza.Core.NotificationManagement.Dtos;
using Peyza.Core.NotificationManagement.Providers;
using System;
using System.Threading;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Entities;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Guids;
using Volo.Abp.Timing;

namespace Peyza.Core.NotificationManagement;

public class SendNotificationHandler : ITransientDependency
{
    private readonly IRepository<MessageTemplate, Guid> _templateRepo;
    private readonly IRepository<NotificationMessage, Guid> _messageRepo;

    private readonly IEmailProvider _emailProvider;
    private readonly ISmsProvider _smsProvider;

    private readonly IGuidGenerator _guidGenerator;
    private readonly IClock _clock;

    public SendNotificationHandler(
        IRepository<MessageTemplate, Guid> templateRepo,
        IRepository<NotificationMessage, Guid> messageRepo,
        IEmailProvider emailProvider,
        ISmsProvider smsProvider,
        IGuidGenerator guidGenerator,
        IClock clock)
    {
        _templateRepo = templateRepo;
        _messageRepo = messageRepo;
        _emailProvider = emailProvider;
        _smsProvider = smsProvider;
        _guidGenerator = guidGenerator;
        _clock = clock;
    }

    public async Task<NotificationMessage> HandleAsync(NotificationMessageRequestDto input, CancellationToken ct = default)
    {
        // 1) Resolver contenido final (body/subject) desde template o body directo
        string bodyFinal;
        string? subjectFinal = input.Subject;

        if (input.TemplateId.HasValue)
        {
            var tpl = await _templateRepo.FindAsync(input.TemplateId.Value, cancellationToken: ct);
            if (tpl is null)
                throw new EntityNotFoundException(typeof(MessageTemplate), input.TemplateId.Value);

            // Si manejas placeholders, aquí iría el renderer.
            bodyFinal = tpl.BodyTemplate;

            // SubjectTemplate puede reemplazar el subject enviado
            subjectFinal = tpl.SubjectTemplate ?? subjectFinal;
        }
        else
        {
            if (string.IsNullOrWhiteSpace(input.Body))
                throw new BusinessException("BODY_REQUIRED");

            bodyFinal = input.Body!;
        }

        // 2) Crear entidad (createdAtUtc requerido por tu modelo)
        var msg = new NotificationMessage(
            id: _guidGenerator.Create(),
            channel: input.Channel,
            destination: input.Destination,
            body: bodyFinal,
            createdAtUtc: _clock.Now,
            subject: subjectFinal,
            debtorId: input.DebtorId,
            debtId: input.DebtId,
            paymentOrderId: input.PaymentOrderId,
            templateId: input.TemplateId
        );

        // 3) Programación (si viene scheduledAt y es futuro)
        if (input.ScheduledAt.HasValue)
        {
            var sendAt = input.ScheduledAt.Value;

            // Regla recomendada: si está en el pasado o "ahora", se envía inmediato
            if (sendAt > _clock.Now)
            {
                msg.Schedule(sendAt);
                await _messageRepo.InsertAsync(msg, autoSave: true, cancellationToken: ct);
                return msg;
            }
        }

        // 4) Envío inmediato
        await _messageRepo.InsertAsync(msg, autoSave: true, cancellationToken: ct);

        msg.MarkAsSending();
        await _messageRepo.UpdateAsync(msg, autoSave: true, cancellationToken: ct);

        try
        {
            var providerId = await SendViaProviderAsync(msg, ct);

            msg.MarkAsSent(providerId, _clock.Now);
            await _messageRepo.UpdateAsync(msg, autoSave: true, cancellationToken: ct);
        }
        catch (Exception ex)
        {
            // Mapea a ErrorCode corto (máx 50)
            var code = MapErrorCode(ex);
            msg.MarkAsFailed(code);

            await _messageRepo.UpdateAsync(msg, autoSave: true, cancellationToken: ct);
        }

        return msg;
    }

    private async Task<string> SendViaProviderAsync(NotificationMessage msg, CancellationToken ct)
    {
        return msg.Channel switch
        {
            NotificationChannel.Email => await _emailProvider.SendAsync(msg, ct),
            NotificationChannel.SMS => await _smsProvider.SendAsync(msg, ct),
            _ => throw new BusinessException("CHANNEL_NOT_SUPPORTED")
        };
    }

    private static string MapErrorCode(Exception ex)
    {
        // Manténlo corto por tu restricción max 50
        // Puedes afinar con tipos específicos (SendGridException, HttpRequestException, etc.)
        return "PROVIDER_SEND_FAILED";
    }
}
