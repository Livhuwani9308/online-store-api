namespace online_store_api.Common
{
    public class ServiceResponse<T>
    {
        public bool IsSuccess { get; set; } = false;
        public int StatusCode { get; set; } = 500;
        public string Message { get; set; } = string.Empty;
        public T? Data { get; set; }
    }
}
