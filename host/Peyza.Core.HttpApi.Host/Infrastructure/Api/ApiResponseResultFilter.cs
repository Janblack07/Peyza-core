using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System;

namespace Peyza.Core.Infrastructure.Api
{
    public class ApiResponseResultFilter : IResultFilter
    {
        public void OnResultExecuting(ResultExecutingContext context)
        {
            // Ya es envelope => no tocar
            if (context.Result is ObjectResult obj &&
                obj.Value is not null &&
                obj.Value.GetType().IsGenericType &&
                obj.Value.GetType().GetGenericTypeDefinition() == typeof(ApiResponse<>))
            {
                return;
            }

            // Solo envolvemos 2xx y respuestas de tipo objeto/json
            var statusCode = (context.Result as ObjectResult)?.StatusCode
                             ?? (context.Result as JsonResult)?.StatusCode
                             ?? (context.Result as StatusCodeResult)?.StatusCode;

            // Si no hay status explícito, ASP.NET Core suele asumir 200 en ObjectResult
            statusCode ??= context.Result is ObjectResult or JsonResult ? 200 : statusCode;

            if (statusCode is null || statusCode < 200 || statusCode >= 300)
            {
                return;
            }

            var correlationId = CorrelationIdMiddleware.GetCorrelationId(context.HttpContext);
            var meta = new ApiMeta
            {
                CorrelationId = correlationId == Guid.Empty ? Guid.NewGuid() : correlationId,
                Timestamp = DateTime.UtcNow
            };

            object? data = context.Result switch
            {
                ObjectResult o => o.Value,
                JsonResult j => j.Value,
                _ => null
            };

            // Envolver
            var wrapped = new ApiResponse<object?>
            {
                Success = true,
                Data = data,
                Error = null,
                Meta = meta
            };

            context.Result = new ObjectResult(wrapped) { StatusCode = statusCode };
        }

        public void OnResultExecuted(ResultExecutedContext context) { }
    }
}
