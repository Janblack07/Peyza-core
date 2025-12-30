using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Peyza.Core.NotificationManagement.Providers.SendGrid
{
    public class SendGridAdapter : IEmailProvider, ISmsProvider
    {
        public Task<string> SendAsync(NotificationMessage message, CancellationToken ct = default)
        {
            // llamar a SendGrid y retornar providerMessageId
            throw new NotImplementedException();
        }
    }
}