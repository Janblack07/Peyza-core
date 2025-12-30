using System;
using System.Collections.Generic;
using System.Text;

namespace Peyza.Core.NotificationManagement.Dtos
{
    public class NotificationMessageResponseDto
    {
        public Guid Id { get; set; }
        public NotificationStatus Status { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
