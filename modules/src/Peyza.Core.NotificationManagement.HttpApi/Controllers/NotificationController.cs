using Microsoft.AspNetCore.Mvc;
using Peyza.Core.NotificationManagement.Dtos;
using System;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.AspNetCore.Mvc;

namespace Peyza.Core.NotificationManagement;

[Route("api/notification-management")]
public class NotificationController : AbpController
{
    private readonly INotificationAppService _app;

    public NotificationController(INotificationAppService app)
    {
        _app = app;
    }

    [HttpGet("templates")]
    public Task<PagedResultDto<MessageTemplateResponseDto>> GetTemplatesAsync([FromQuery] GetTemplatesInputDto input)
    => _app.GetTemplatesAsync(input);

    [HttpGet("notifications")]
    public Task<PagedResultDto<NotificationMessageSummaryDto>> GetNotificationsAsync([FromQuery] GetNotificationsInputDto input)
        => _app.GetNotificationsAsync(input);

    [HttpGet("templates/{id:guid}")]
    public Task<MessageTemplateResponseDto> GetTemplate(Guid id)
        => _app.GetTemplateAsync(id);

    [HttpPost("notifications/send")]
    public Task<NotificationMessageResponseDto> Send([FromBody] NotificationMessageRequestDto input)
        => _app.SendAsync(input);

    [HttpGet("notifications/{id:guid}")]
    public Task<NotificationMessageDetailsDto> GetNotificationAsync(Guid id)
    => _app.GetNotificationAsync(id);


    [HttpPost("templates/{id:guid}/activate")]
    public Task<MessageTemplateResponseDto> ActivateTemplateAsync(Guid id)
    => _app.ActivateTemplateAsync(id);

    [HttpPost("templates/{id:guid}/deprecate")]
    public Task<MessageTemplateResponseDto> DeprecateTemplateAsync(Guid id)
        => _app.DeprecateTemplateAsync(id);

    [HttpPost("notifications/{id:guid}/retry")]
    public Task<NotificationMessageDetailsDto> RetryAsync(Guid id)
    => _app.RetryNotificationAsync(id);
}