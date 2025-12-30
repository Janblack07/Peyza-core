using System;
using System.Collections.Generic;
using System.Text;
using Volo.Abp.Application.Dtos;

namespace Peyza.Core.NotificationManagement.Dtos
{
    public class GetNotificationsInputDto : PagedAndSortedResultRequestDto
    {
        public NotificationStatus? Status { get; set; }
        public NotificationChannel? Channel { get; set; }
        public string? Destination { get; set; }

        public DateTime? From { get; set; }
        public DateTime? To { get; set; }

        public string? Filter { get; set; }
    }
}
