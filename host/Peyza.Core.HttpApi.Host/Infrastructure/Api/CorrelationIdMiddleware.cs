using Microsoft.AspNetCore.Http;
using System;
using System.Threading.Tasks;

namespace Peyza.Core.Infrastructure.Api
{
    public class CorrelationIdMiddleware : IMiddleware
    {
        public const string HeaderName = "X-Correlation-ID";
        public const string ItemKey = "CorrelationId";

        public async Task InvokeAsync(HttpContext context, RequestDelegate next)
        {
            Guid correlationId;

            if (context.Request.Headers.TryGetValue(HeaderName, out var values) &&
                Guid.TryParse(values.ToString(), out var parsed))
            {
                correlationId = parsed;
            }
            else
            {
                correlationId = Guid.NewGuid();
            }

            context.Items[ItemKey] = correlationId;
            context.Response.Headers[HeaderName] = correlationId.ToString();

            await next(context);
        }

        public static Guid GetCorrelationId(HttpContext ctx)
            => ctx.Items.TryGetValue(ItemKey, out var value) && value is Guid g ? g : Guid.Empty;
    }
}
