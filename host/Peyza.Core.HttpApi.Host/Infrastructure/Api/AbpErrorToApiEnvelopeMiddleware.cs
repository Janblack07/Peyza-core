using Microsoft.AspNetCore.Http;
using System;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Peyza.Core.Infrastructure.Api
{
    public class AbpErrorToApiEnvelopeMiddleware : IMiddleware
    {
        public async Task InvokeAsync(HttpContext context, RequestDelegate next)
        {
            // Intercepta el body de respuesta
            var originalBody = context.Response.Body;
            await using var mem = new MemoryStream();
            context.Response.Body = mem;

            try
            {
                await next(context);

                // Solo rewrap en errores (>=400) y JSON
                if (context.Response.StatusCode < 400)
                    return;

                mem.Position = 0;
                var raw = await new StreamReader(mem, Encoding.UTF8).ReadToEndAsync();

                // Si no es error ABP, no tocar
                if (!context.Response.Headers.ContainsKey("_abperrorformat") || string.IsNullOrWhiteSpace(raw))
                    return;

                // Parsear error ABP: { "error": { ... } }
                string message = "An error occurred.";
                string code = MapCode(context.Response.StatusCode);
                object? details = null;

                try
                {
                    using var doc = JsonDocument.Parse(raw);
                    if (doc.RootElement.TryGetProperty("error", out var err))
                    {
                        if (err.TryGetProperty("message", out var m) && m.ValueKind == JsonValueKind.String)
                            message = m.GetString() ?? message;

                        if (err.TryGetProperty("code", out var c) && c.ValueKind == JsonValueKind.String)
                            code = c.GetString() ?? code;

                        // validationErrors (si viniera)
                        if (err.TryGetProperty("validationErrors", out var ve) && ve.ValueKind != JsonValueKind.Null)
                            details = ve;
                    }
                }
                catch
                {
                    // si no parsea, usamos raw como mensaje
                    message = raw.Length > 200 ? raw[..200] : raw;
                }

                var correlationId = CorrelationIdMiddleware.GetCorrelationId(context);
                var meta = new ApiMeta
                {
                    CorrelationId = correlationId == Guid.Empty ? Guid.NewGuid() : correlationId,
                    Timestamp = DateTime.UtcNow
                };

                var wrapped = new ApiResponse<object?>
                {
                    Success = false,
                    Data = null,
                    Error = new ApiError
                    {
                        Code = string.IsNullOrWhiteSpace(code) ? MapCode(context.Response.StatusCode) : code,
                        HttpStatus = context.Response.StatusCode,
                        Message = message,
                        Details = null // si quieres mapear validationErrors a ApiErrorDetail lo hacemos luego
                    },
                    Meta = meta
                };

                // Reescribir respuesta
                context.Response.Headers.Remove("_abperrorformat");
                context.Response.ContentType = "application/json; charset=utf-8";

                var json = JsonSerializer.Serialize(wrapped);
                var bytes = Encoding.UTF8.GetBytes(json);

                context.Response.Body = originalBody;
                context.Response.ContentLength = bytes.Length;
                await context.Response.Body.WriteAsync(bytes, 0, bytes.Length);
            }
            finally
            {
                // Si no reescribimos, devolver el body original tal cual
                if (context.Response.Body == mem)
                {
                    mem.Position = 0;
                    await mem.CopyToAsync(originalBody);
                    context.Response.Body = originalBody;
                }
            }
        }

        private static string MapCode(int statusCode) => statusCode switch
        {
            400 => "VALIDATION_ERROR",
            401 => "UNAUTHORIZED",
            403 => "FORBIDDEN",
            404 => "NOT_FOUND",
            _ => "UNEXPECTED_ERROR"
        };
    }
}
