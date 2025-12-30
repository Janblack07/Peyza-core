using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Peyza.Core.NotificationManagement.Providers.SendGrid
{
    public class SendGridNotificationProviderDispatcher : INotificationProviderDispatcher
    {
        public const string HttpClientName = "SendGrid";

        private readonly IHttpClientFactory _httpClientFactory;
        private readonly SendGridOptions _options;
        private readonly ILogger<SendGridNotificationProviderDispatcher> _logger;

        public SendGridNotificationProviderDispatcher(
            IHttpClientFactory httpClientFactory,
            IOptions<SendGridOptions> options,
            ILogger<SendGridNotificationProviderDispatcher> logger)
        {
            _httpClientFactory = httpClientFactory;
            _options = options.Value;
            _logger = logger;
        }

        public async Task<ProviderSendResult> SendAsync(NotificationMessage message, CancellationToken cancellationToken = default)
        {
            if (message.Channel != NotificationChannel.Email)
            {
                return new ProviderSendResult(
                    Success: false,
                    ProviderMessageId: null,
                    ErrorCode: "CHANNEL_NOT_SUPPORTED"
                );
            }

            if (string.IsNullOrWhiteSpace(_options.ApiKey) || string.IsNullOrWhiteSpace(_options.FromEmail))
            {
                return new ProviderSendResult(false, null, "SENDGRID_NOT_CONFIGURED");
            }

            var client = _httpClientFactory.CreateClient(HttpClientName);

            var payload = new
            {
                personalizations = new[]
                   {
                     new
                        {
                            to = new[] { new { email = message.Destination } },
                            subject = message.Subject ?? "(no-subject)",
                            custom_args = new
                            {
                                notificationMessageId = message.Id.ToString()
                            }
                        }
                   },
                     from = new
                         {
                               email = _options.FromEmail,
                               name = _options.FromName
                         },
                               content = new[]
                    {
                        new { type = "text/html", value = message.Body }
                    }
            };

            using var req = new HttpRequestMessage(HttpMethod.Post, "/v3/mail/send");
            req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _options.ApiKey);
            req.Content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");

            try
            {
                var resp = await client.SendAsync(req, cancellationToken);

                if (resp.IsSuccessStatusCode)
                {
                    // SendGrid a veces retorna cabeceras útiles; si no, generamos un correlativo.
                    var providerId =
                        resp.Headers.TryGetValues("X-Message-Id", out var values)
                            ? values.FirstOrDefault()
                            : $"sendgrid:{message.Id:N}";

                    return new ProviderSendResult(true, providerId, null);
                }

                var code = $"SENDGRID_HTTP_{(int)resp.StatusCode}";
                _logger.LogWarning("SendGrid failed. Status={StatusCode}, MessageId={MessageId}", (int)resp.StatusCode, message.Id);

                return new ProviderSendResult(false, null, code);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "SendGrid exception. MessageId={MessageId}", message.Id);
                return new ProviderSendResult(false, null, "SENDGRID_EXCEPTION");
            }
        }
    }
}
