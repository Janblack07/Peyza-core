using System;

namespace Peyza.Core.Infrastructure.Api
{
    public class ApiMeta
    {
        public Guid CorrelationId { get; set; }
        public DateTime Timestamp { get; set; }
    }
}
