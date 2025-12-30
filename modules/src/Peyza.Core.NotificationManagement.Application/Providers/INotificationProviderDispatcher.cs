using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Peyza.Core.NotificationManagement.Providers
{
    public interface INotificationProviderDispatcher
    {
        Task<ProviderSendResult> SendAsync(NotificationMessage message, CancellationToken cancellationToken = default);
    }
}
