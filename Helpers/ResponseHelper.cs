using online_store_api.Common;

namespace online_store_api.Helpers
{
    public class ResponseHelper : IResponseHelper
    {
        public ServiceResponse<T> Create<T>(bool isSuccess, int statusCode, string message, T? data)
        {
            return new ServiceResponse<T>
            {
                IsSuccess = isSuccess,
                StatusCode = statusCode,
                Message = message,
                Data = data
            };
        }
    }

    public interface IResponseHelper
    {
        ServiceResponse<T> Create<T>(bool isSuccess, int statusCode, string message, T? data);
    }
}
