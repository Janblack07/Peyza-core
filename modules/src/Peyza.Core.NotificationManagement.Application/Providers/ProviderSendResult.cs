using System;
using System.Collections.Generic;
using System.Text;

namespace Peyza.Core.NotificationManagement.Providers
{
    public sealed record ProviderSendResult(
    bool Success,
    string? ProviderMessageId,
    string? ErrorCode
);
}
