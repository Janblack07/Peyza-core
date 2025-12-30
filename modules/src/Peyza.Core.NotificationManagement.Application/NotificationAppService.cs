using System;
using System.Threading.Tasks;
using Peyza.Core.NotificationManagement.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;

namespace Peyza.Core.NotificationManagement;

public class NotificationAppService : ApplicationService, INotificationAppService
{
    private readonly IRepository<MessageTemplate, Guid> _templateRepo;
    private readonly IRepository<NotificationMessage, Guid> _messageRepo;

    public NotificationAppService(
        IRepository<MessageTemplate, Guid> templateRepo,
        IRepository<NotificationMessage, Guid> messageRepo)
    {
        _templateRepo = templateRepo;
        _messageRepo = messageRepo;
    }

    public async Task<MessageTemplateResponseDto> CreateTemplateAsync(MessageTemplateRequestDto input)
    {
        var id = GuidGenerator.Create();

        // Constructor real de tu entidad:
        // MessageTemplate(Guid id, string name, string? code, NotificationChannel channel, string bodyTemplate)
        var template = new MessageTemplate(
            id,
            input.Name,
            input.Code,
            input.Channel,
            input.BodyTemplate
        );

        // Completar campos opcionales si vienen
        if (input.SubjectTemplate is not null || input.Language is not null || input.Code is not null)
        {
            template.UpdateContent(
                bodyTemplate: input.BodyTemplate,
                subjectTemplate: input.SubjectTemplate,
                language: input.Language,
                code: input.Code
            );
        }

        await _templateRepo.InsertAsync(template, autoSave: true);

        return Map(template);
    }

    public async Task<MessageTemplateResponseDto> GetTemplateAsync(Guid id)
    {
        var template = await _templateRepo.GetAsync(id);
        return Map(template);
    }

    public async Task<NotificationMessageResponseDto> SendAsync(NotificationMessageRequestDto input)
    {
        var id = GuidGenerator.Create();

        // Constructor real de tu entidad:
        // NotificationMessage(Guid id, NotificationChannel channel, string destination, string body, string? subject = null, Guid? debtorId=null, ...)
        var msg = new NotificationMessage(
            id,
            input.Channel,
            input.Destination,
            input.Body,
            input.Subject,
            input.DebtorId,
            input.DebtId,
            input.PaymentOrderId,
            input.TemplateId
        );

        // Si viene programado, aplicar regla de dominio
        if (input.ScheduledAt.HasValue)
        {
            // Asumimos que input.ScheduledAt ya viene en UTC o que el sistema trabaja en UTC.
            msg.Schedule(input.ScheduledAt.Value);
        }

        await _messageRepo.InsertAsync(msg, autoSave: true);

        return new NotificationMessageResponseDto
        {
            Id = msg.Id,
            Status = msg.Status,
            CreatedAt = msg.CreatedAt
        };
    }

    private static MessageTemplateResponseDto Map(MessageTemplate t) => new()
    {
        Id = t.Id,
        Name = t.Name,
        Code = t.Code,
        Channel = t.Channel,
        Language = t.Language,
        SubjectTemplate = t.SubjectTemplate,
        BodyTemplate = t.BodyTemplate,
        Status = t.Status
   
    };
}

