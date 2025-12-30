using System.Collections.Generic;

namespace Peyza.Core.Infrastructure.Api
{
    public class ApiError
    {
        public string Code { get; set; } = "ERROR";
        public int HttpStatus { get; set; }
        public string Message { get; set; } = "An error occurred.";
        public List<ApiErrorDetail>? Details { get; set; }
    }

    public class ApiErrorDetail
    {
        public string? Target { get; set; }
        public string? Code { get; set; }
        public string? Message { get; set; }
    }
}
