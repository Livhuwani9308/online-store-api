using online_store_api.Common;

namespace online_store_api.Helpers
{
    public class ResponseHelper : IResponseHelper
    {
        public ServiceResponse<T> CreateResponse<T>(bool isSuccess, int statusCode, string message, T? data)
        {
            return new ServiceResponse<T>
            {
                IsSuccess = isSuccess,
                StatusCode = statusCode,
                Message = message,
                Data = data
            };
        }

        public ServiceResponseWithToken<T> CreateResponse<T>(bool isSuccess, int statusCode, string message, T? data, string token)
        {
            return new ServiceResponseWithToken<T>
            {
                IsSuccess = isSuccess,
                StatusCode = statusCode,
                Message = message,
                Data = data,
                Token = token
            };
        }
    }

    public interface IResponseHelper
    {
        ServiceResponse<T> CreateResponse<T>(bool isSuccess, int statusCode, string message, T? data);
        ServiceResponseWithToken<T> CreateResponse<T>(bool isSuccess, int statusCode, string message, T? data, string token);
    }
}
