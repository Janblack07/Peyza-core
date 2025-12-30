using Peyza.Core.NotificationManagement.Dtos;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace Peyza.Core.NotificationManagement
{
    public interface INotificationAppService : IApplicationService
    {
        Task<NotificationMessageResponseDto> SendAsync(NotificationMessageRequestDto input);

        Task<MessageTemplateResponseDto> CreateTemplateAsync(MessageTemplateRequestDto input);

        Task<MessageTemplateResponseDto> GetTemplateAsync(Guid id);
        Task<NotificationMessageDetailsDto> GetNotificationAsync(Guid id);
        Task<PagedResultDto<MessageTemplateResponseDto>> GetTemplatesAsync(GetTemplatesInputDto input);
        Task<PagedResultDto<NotificationMessageSummaryDto>> GetNotificationsAsync(GetNotificationsInputDto input);
        Task<MessageTemplateResponseDto> ActivateTemplateAsync(Guid id);
        Task<MessageTemplateResponseDto> DeprecateTemplateAsync(Guid id);
        Task<NotificationMessageDetailsDto> RetryNotificationAsync(Guid id);


    }
}
