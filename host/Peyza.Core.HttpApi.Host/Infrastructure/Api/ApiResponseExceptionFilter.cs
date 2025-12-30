using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System;
using System.Linq;
using Volo.Abp;
using Volo.Abp.Authorization;
using Volo.Abp.Domain.Entities;
using Volo.Abp.Validation;

namespace Peyza.Core.Infrastructure.Api
{
    public class ApiResponseExceptionFilter : IExceptionFilter
    {
        public void OnException(ExceptionContext context)
        {
            var ex = context.Exception;

            var (status, code, message, details) = MapException(ex);

            var correlationId = CorrelationIdMiddleware.GetCorrelationId(context.HttpContext);
            var meta = new ApiMeta
            {
                CorrelationId = correlationId == Guid.Empty ? Guid.NewGuid() : correlationId,
                Timestamp = DateTime.UtcNow
            };

            var response = new ApiResponse<object?>
            {
                Success = false,
                Data = null,
                Error = new ApiError
                {
                    Code = code,
                    HttpStatus = status,
                    Message = message,
                    Details = details
                },
                Meta = meta
            };

            context.Result = new ObjectResult(response) { StatusCode = status };
            context.ExceptionHandled = true;
        }

        private static (int status, string code, string message, System.Collections.Generic.List<ApiErrorDetail>? details) MapException(Exception ex)
        {
            // Validaciones ABP
            if (ex is AbpValidationException vex)
            {
                var details = vex.ValidationErrors?
                    .Select(e => new ApiErrorDetail
                    {
                        Target = e.MemberNames?.FirstOrDefault(),
                        Code = "VALIDATION",
                        Message = e.ErrorMessage
                    })
                    .ToList();

                return (400, "VALIDATION_ERROR", "One or more validation errors occurred.", details);
            }

            // Negocio
            if (ex is BusinessException bex)
            {
                var code = string.IsNullOrWhiteSpace(bex.Code) ? "BUSINESS_ERROR" : bex.Code!;
                var msg = string.IsNullOrWhiteSpace(bex.Message) ? "Business rule violated." : bex.Message!;
                return (400, code, msg, null);
            }

            // Not found
            if (ex is EntityNotFoundException)
            {
                return (404, "NOT_FOUND", "Resource not found.", null);
            }

            // Auth (si aplica)
            if (ex is AbpAuthorizationException)
            {
                return (403, "FORBIDDEN", "Access denied.", null);
            }

            // Genérico
            return (500, "UNEXPECTED_ERROR", "An unexpected error occurred.", null);
        }
    }
}
