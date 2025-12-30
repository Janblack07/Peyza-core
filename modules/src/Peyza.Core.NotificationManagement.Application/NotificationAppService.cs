using Peyza.Core.NotificationManagement.Dtos;
using Peyza.Core.NotificationManagement.Handlers;
using System;
using System.Threading.Tasks;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;

namespace Peyza.Core.NotificationManagement;

public class NotificationAppService : ApplicationService, INotificationAppService
{
    private readonly IRepository<MessageTemplate, Guid> _templateRepo;
    private readonly SendNotificationHandler _sendHandler;

    public NotificationAppService(
        IRepository<MessageTemplate, Guid> templateRepo,
        SendNotificationHandler sendHandler)
    {
        _templateRepo = templateRepo;
        _sendHandler = sendHandler;
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
        if (input.SubjectTemplate is not null  || input.Code is not null)
        {
            template.UpdateContent(
                bodyTemplate: input.BodyTemplate,
                subjectTemplate: input.SubjectTemplate,
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

    public Task<NotificationMessageResponseDto> SendAsync(NotificationMessageRequestDto input)
        => _sendHandler.HandleAsync(input);


    private static MessageTemplateResponseDto Map(MessageTemplate t) => new()
    {
        Id = t.Id,
        Name = t.Name,
        Code = t.Code,
        Channel = t.Channel,
        SubjectTemplate = t.SubjectTemplate,
        BodyTemplate = t.BodyTemplate,
        Status = t.Status
   
    };
}

