using System;
using System.Collections.Generic;
using System.Text;
using Volo.Abp.Application.Dtos;

namespace Peyza.Core.NotificationManagement.Dtos
{
    public class GetTemplatesInputDto : PagedAndSortedResultRequestDto
    {
        public NotificationChannel? Channel { get; set; }
        public TemplateStatus? Status { get; set; }
        public string? Filter { get; set; }
    }
}
