using System;
using System.Collections.Generic;
using System.Text;

namespace Peyza.Core.NotificationManagement.Dtos
{
    public class MessageTemplateRequestDto
    {
        public string Name { get; set; } = default!;
        public string? Code { get; set; }
        public NotificationChannel Channel { get; set; }
        public string? SubjectTemplate { get; set; }
        public string BodyTemplate { get; set; } = default!;
    }
}
