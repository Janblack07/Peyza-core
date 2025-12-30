using Microsoft.AspNetCore.Http;
using System;
using System.Text.Json;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Authorization;
using Volo.Abp.Domain.Entities;
using Volo.Abp.Validation;

namespace Peyza.Core.Infrastructure.Api
{
    public class ApiExceptionEnvelopeMiddleware : IMiddleware
    {
        public async Task InvokeAsync(HttpContext context, RequestDelegate next)
        {
            try
            {
                await next(context);
            }
            catch (Exception ex)
            {
                var (status, code, message) = Map(ex);

                var correlationId = CorrelationIdMiddleware.GetCorrelationId(context);
                var meta = new ApiMeta
                {
                    CorrelationId = correlationId == Guid.Empty ? Guid.NewGuid() : correlationId,
                    Timestamp = DateTime.UtcNow
                };

                var body = new ApiResponse<object?>
                {
                    Success = false,
                    Data = null,
                    Error = new ApiError
                    {
                        Code = code,
                        HttpStatus = status,
                        Message = message
                    },
                    Meta = meta
                };

                context.Response.StatusCode = status;
                context.Response.ContentType = "application/json; charset=utf-8";
                await context.Response.WriteAsync(JsonSerializer.Serialize(body));
            }
        }

        private static (int status, string code, string message) Map(Exception ex)
        {
            if (ex is AbpValidationException) return (400, "VALIDATION_ERROR", "One or more validation errors occurred.");
            if (ex is BusinessException bex) return (400, bex.Code ?? "BUSINESS_ERROR", bex.Message);
            if (ex is EntityNotFoundException) return (404, "NOT_FOUND", "Resource not found.");
            if (ex is AbpAuthorizationException) return (403, "FORBIDDEN", "Access denied.");
            return (500, "UNEXPECTED_ERROR", "An unexpected error occurred.");
        }
    }
}