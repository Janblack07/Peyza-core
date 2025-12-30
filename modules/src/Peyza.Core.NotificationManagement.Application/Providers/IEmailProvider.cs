using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Peyza.Core.NotificationManagement.Providers
{
    public interface IEmailProvider
    {
        Task<string> SendAsync(NotificationMessage message, CancellationToken ct = default);
    }
}
