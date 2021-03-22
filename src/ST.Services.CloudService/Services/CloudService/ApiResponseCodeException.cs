using System.Application.Models;

namespace System.Application.Services.CloudService
{
    internal sealed class ApiResponseCodeException : Exception
    {
        public ApiResponseCodeException(ApiResponseCode code) : base(ApiResponse.GetMessage(code))
            => Code = code;

        public ApiResponseCodeException(ApiResponseCode code, string message) : base(message)
            => Code = code;

        public ApiResponseCodeException(ApiResponseCode code, string message, Exception innerException) : base(message, innerException)
            => Code = code;

        public ApiResponseCode Code { get; }
    }
}