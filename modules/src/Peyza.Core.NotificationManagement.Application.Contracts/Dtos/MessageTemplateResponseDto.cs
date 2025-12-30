using System;
using System.Collections.Generic;
using System.Text;

namespace Peyza.Core.NotificationManagement.Dtos
{
    public class MessageTemplateResponseDto : MessageTemplateRequestDto
    {
        public Guid Id { get; set; }
        public TemplateStatus Status { get; set; }
    }
}
