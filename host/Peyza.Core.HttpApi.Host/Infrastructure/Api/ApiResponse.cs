namespace Peyza.Core.Infrastructure.Api
{
    public class ApiResponse<T>
    {
        public bool Success { get; set; }
        public T? Data { get; set; }
        public ApiError? Error { get; set; }
        public ApiMeta Meta { get; set; } = new();
    }
}
