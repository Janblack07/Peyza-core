using Peyza.Core.NotificationManagement.Dtos;
using Peyza.Core.NotificationManagement.Handlers;
using System;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Entities;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Timing;

namespace Peyza.Core.NotificationManagement;



public class NotificationAppService : ApplicationService, INotificationAppService
{
    private readonly IRepository<MessageTemplate, Guid> _templateRepo;
    private readonly SendNotificationHandler _sendHandler;
    private readonly IRepository<NotificationMessage, Guid> _messageRepo;
    private readonly IClock _clock;

    public NotificationAppService(
        IRepository<MessageTemplate, Guid> templateRepo,
        IRepository<NotificationMessage, Guid> messageRepo,
        SendNotificationHandler sendHandler, IClock clock)
    {
        _templateRepo = templateRepo;
        _messageRepo = messageRepo;
        _sendHandler = sendHandler;
        _clock = clock;
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
    public async Task<NotificationMessageDetailsDto> GetNotificationAsync(Guid id)
    {
        var msg = await _messageRepo.GetAsync(id);

        return new NotificationMessageDetailsDto
        {
            Id = msg.Id,
            Status = msg.Status,
            CreatedAt = msg.CreatedAt,
            ScheduledAt = msg.ScheduledAt,
            SentAt = msg.SentAt,
            ProviderMessageId = msg.ProviderMessageId,
            ErrorCode = msg.ErrorCode
        };
    }
   
   
    public async Task<MessageTemplateResponseDto> GetTemplateAsync(Guid id)
    {
        var template = await _templateRepo.GetAsync(id);
        return Map(template);
    }

    public async Task<NotificationMessageResponseDto> SendAsync(NotificationMessageRequestDto input)
    {
        // Handler devuelve ENTIDAD
        var msg = await _sendHandler.HandleAsync(input);

        // AppService devuelve DTO (esto elimina el error de conversión)
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
        SubjectTemplate = t.SubjectTemplate,
        BodyTemplate = t.BodyTemplate,
        Status = t.Status
   
    };
    public async Task<PagedResultDto<MessageTemplateResponseDto>> GetTemplatesAsync(GetTemplatesInputDto input)
    {
        var q = await _templateRepo.GetQueryableAsync();

        if (input.Channel.HasValue)
            q = q.Where(x => x.Channel == input.Channel.Value);

        if (input.Status.HasValue)
            q = q.Where(x => x.Status == input.Status.Value);

        if (!string.IsNullOrWhiteSpace(input.Filter))
            q = q.Where(x => x.Name.Contains(input.Filter));

        var totalCount = await AsyncExecuter.CountAsync(q);

        // Sorting simple (sin Dynamic LINQ)
        // Por defecto: Name asc
        var sorting = (input.Sorting ?? "").Trim().ToLowerInvariant();
        q = sorting switch
        {
            "name desc" => q.OrderByDescending(x => x.Name),
            "createdat asc" => q.OrderBy(x => x.CreatedAt),
            "createdat desc" => q.OrderByDescending(x => x.CreatedAt),
            _ => q.OrderBy(x => x.Name)
        };

        var entities = await AsyncExecuter.ToListAsync(
            q.Skip(input.SkipCount).Take(input.MaxResultCount)
        );

        var items = entities.Select(x => new MessageTemplateResponseDto
        {
            Id = x.Id,
            Name = x.Name,
            Code = x.Code,
            Channel = x.Channel,
            SubjectTemplate = x.SubjectTemplate,
            BodyTemplate = x.BodyTemplate,
            Status = x.Status
        }).ToList();

        return new PagedResultDto<MessageTemplateResponseDto>(totalCount, items);
    }
    public async Task<PagedResultDto<NotificationMessageSummaryDto>> GetNotificationsAsync(GetNotificationsInputDto input)
    {
        var q = await _messageRepo.GetQueryableAsync();

        if (input.Status.HasValue)
            q = q.Where(x => x.Status == input.Status.Value);

        if (input.Channel.HasValue)
            q = q.Where(x => x.Channel == input.Channel.Value);

        if (!string.IsNullOrWhiteSpace(input.Destination))
            q = q.Where(x => x.Destination.Contains(input.Destination));

        if (input.From.HasValue)
            q = q.Where(x => x.CreatedAt >= input.From.Value);

        if (input.To.HasValue)
            q = q.Where(x => x.CreatedAt <= input.To.Value);

        if (!string.IsNullOrWhiteSpace(input.Filter))
            q = q.Where(x =>
                (x.Subject != null && x.Subject.Contains(input.Filter)) ||
                x.Destination.Contains(input.Filter));

        var totalCount = await AsyncExecuter.CountAsync(q);

        // Sorting simple (sin Dynamic LINQ)
        // Por defecto: CreatedAt desc (más nuevo primero)
        var sorting = (input.Sorting ?? "").Trim().ToLowerInvariant();
        q = sorting switch
        {
            "createdat asc" => q.OrderBy(x => x.CreatedAt),
            "createdat desc" => q.OrderByDescending(x => x.CreatedAt),
            "status asc" => q.OrderBy(x => x.Status),
            "status desc" => q.OrderByDescending(x => x.Status),
            _ => q.OrderByDescending(x => x.CreatedAt)
        };

        var entities = await AsyncExecuter.ToListAsync(
            q.Skip(input.SkipCount).Take(input.MaxResultCount)
        );

        var items = entities.Select(x => new NotificationMessageSummaryDto
        {
            Id = x.Id,
            Channel = x.Channel,
            Destination = x.Destination,
            Subject = x.Subject,
            Status = x.Status,
            CreatedAt = x.CreatedAt,
            ScheduledAt = x.ScheduledAt,
            SentAt = x.SentAt
        }).ToList();

        return new PagedResultDto<NotificationMessageSummaryDto>(totalCount, items);
    }

    public async Task<MessageTemplateResponseDto> ActivateTemplateAsync(Guid id)
    {
        var tpl = await _templateRepo.FindAsync(id);
        if (tpl is null)
            throw new EntityNotFoundException(typeof(MessageTemplate), id);

        tpl.Activate();

        await _templateRepo.UpdateAsync(tpl, autoSave: true);

        return new MessageTemplateResponseDto
        {
            Id = tpl.Id,
            Name = tpl.Name,
            Code = tpl.Code,
            Channel = tpl.Channel,
            SubjectTemplate = tpl.SubjectTemplate,
            BodyTemplate = tpl.BodyTemplate,
            Status = tpl.Status
        };
    }

    public async Task<MessageTemplateResponseDto> DeprecateTemplateAsync(Guid id)
    {
        var tpl = await _templateRepo.FindAsync(id);
        if (tpl is null)
            throw new EntityNotFoundException(typeof(MessageTemplate), id);

        tpl.Deprecate();

        await _templateRepo.UpdateAsync(tpl, autoSave: true);

        return new MessageTemplateResponseDto
        {
            Id = tpl.Id,
            Name = tpl.Name,
            Code = tpl.Code,
            Channel = tpl.Channel,
            SubjectTemplate = tpl.SubjectTemplate,
            BodyTemplate = tpl.BodyTemplate,
            Status = tpl.Status
        };
    }
    public async Task<NotificationMessageDetailsDto> RetryNotificationAsync(Guid id)
    {
        var msg = await _messageRepo.FindAsync(id);
        if (msg is null)
            throw new EntityNotFoundException(typeof(NotificationMessage), id);

        msg.Retry(_clock.Now);
        await _messageRepo.UpdateAsync(msg, autoSave: true);

        return await GetNotificationAsync(id);
    }
}

