using System;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Peyza.Core.NotificationManagement.Handlers;
using Volo.Abp.AspNetCore.Mvc;

namespace Peyza.Core.NotificationManagement.Controllers { 

[AllowAnonymous]
[Route("api/notification-management/webhooks/sendgrid")]
public class SendGridWebhookController : AbpController
{
    private readonly UpdateDeliveryStatusHandler _handler;

    public SendGridWebhookController(UpdateDeliveryStatusHandler handler)
    {
        _handler = handler;
    }

    [HttpPost]
    public async Task<IActionResult> Receive()
    {
        using var reader = new StreamReader(Request.Body);
        var raw = await reader.ReadToEndAsync();

        if (string.IsNullOrWhiteSpace(raw))
            return Ok();

        // SendGrid envía un array de eventos
        using var doc = JsonDocument.Parse(raw);

        if (doc.RootElement.ValueKind != JsonValueKind.Array)
            return Ok();

        foreach (var ev in doc.RootElement.EnumerateArray())
        {
            // event
            var eventType = ev.TryGetProperty("event", out var e) ? e.GetString() : null;

            // sg_message_id (puede venir)
            var sgId = ev.TryGetProperty("sg_message_id", out var sg) ? sg.GetString() : null;

            // custom_args.notificationMessageId (lo que nosotros enviamos)
            Guid messageId = default;
            if (ev.TryGetProperty("custom_args", out var ca) &&
                ca.ValueKind == JsonValueKind.Object &&
                ca.TryGetProperty("notificationMessageId", out var mid))
            {
                Guid.TryParse(mid.GetString(), out messageId);
            }

            if (messageId == default)
                continue;

            await _handler.HandleAsync(messageId, eventType ?? "", sgId, reason: null, HttpContext.RequestAborted);
        }

        return Ok();
    }
}
}