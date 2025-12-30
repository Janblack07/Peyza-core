using System;
using System.Collections.Generic;
using System.Text;
using Volo.Abp;
using Volo.Abp.Domain.Entities;

namespace Peyza.Core.NotificationManagement
{
    public class MessageTemplate : AggregateRoot<Guid>
    {
        public string Name { get; private set; } = default!;
        public string? Code { get; private set; }
        public NotificationChannel Channel { get; private set; }
        public string? Language { get; private set; }
        public string? SubjectTemplate { get; private set; }
        public string BodyTemplate { get; private set; } = default!;
        public TemplateStatus Status { get; private set; }
        public DateTime CreatedAt { get; private set; }

        private MessageTemplate() { }

        public MessageTemplate(Guid id, string name, string? code, NotificationChannel channel, string bodyTemplate) : base(id)
        {
            Name = Check.NotNullOrWhiteSpace(name, nameof(name), maxLength: 100);
            Code = code;
            Channel = channel;
            BodyTemplate = Check.NotNullOrWhiteSpace(bodyTemplate, nameof(bodyTemplate));
            CreatedAt = DateTime.UtcNow;
            Status = TemplateStatus.Draft;
        }

        public MessageTemplate(Guid id, string name, NotificationChannel channel, string bodyTemplate) : base(id)
        {
        }

        public void UpdateContent(string bodyTemplate, string? subjectTemplate = null, string? language = null, string? code = null)
        {
            if (Status == TemplateStatus.Deprecated)
                throw new BusinessException("TEMPLATE_DEPRECATED");

            BodyTemplate = Check.NotNullOrWhiteSpace(bodyTemplate, nameof(bodyTemplate));
            SubjectTemplate = subjectTemplate is null ? null : Check.Length(subjectTemplate, nameof(subjectTemplate), maxLength: 200);
            Language = language is null ? null : Check.Length(language, nameof(language), maxLength: 20);
            Code = code is null ? null : Check.Length(code, nameof(code), maxLength: 100);
        }

        public void Activate()
        {
            if (Status == TemplateStatus.Deprecated)
                throw new BusinessException("TEMPLATE_DEPRECATED_CANNOT_ACTIVATE");

            Status = TemplateStatus.Active;
        }

        public void Deprecate() => Status = TemplateStatus.Deprecated;
    }
}
