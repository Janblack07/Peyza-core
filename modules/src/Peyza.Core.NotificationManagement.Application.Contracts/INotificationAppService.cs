using Peyza.Core.NotificationManagement.Dtos;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Application.Services;

namespace Peyza.Core.NotificationManagement
{
    public interface INotificationAppService : IApplicationService
    {
        Task<NotificationMessageResponseDto> SendAsync(NotificationMessageRequestDto input);

        Task<MessageTemplateResponseDto> CreateTemplateAsync(MessageTemplateRequestDto input);

        Task<MessageTemplateResponseDto> GetTemplateAsync(Guid id);
    }
}
