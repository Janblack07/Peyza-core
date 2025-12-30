using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Peyza.Core.NotificationManagement.Dtos;
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

    [HttpPost("templates")]
    public Task<MessageTemplateResponseDto> CreateTemplate([FromBody] MessageTemplateRequestDto input)
     => _app.CreateTemplateAsync(input);

    [HttpGet("templates/{id:guid}")]
    public Task<MessageTemplateResponseDto> GetTemplate(Guid id)
        => _app.GetTemplateAsync(id);

    [HttpPost("notifications/send")]
    public Task<NotificationMessageResponseDto> Send([FromBody] NotificationMessageRequestDto input)
        => _app.SendAsync(input);
}