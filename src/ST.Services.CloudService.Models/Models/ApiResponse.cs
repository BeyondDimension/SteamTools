using MessagePack;
using Newtonsoft.Json;
using System.Application.Models.Internals;
using System.Application.Properties;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace System.Application.Models
{
    public static class ApiResponse
    {
        static IApiResponse<T> ClientDeserializeFail<T>() => Code<T>(ApiResponseCode.ClientDeserializeFail);

        static Type GetDeserializeType<T>()
        {
            if (typeof(T) == typeof(object)) return typeof(ApiResponseImpl);
            return typeof(ApiResponseImpl<T>);
        }

        public static IApiResponse<T> Deserialize<T>(
            JsonSerializer jsonSerializer,
            JsonReader jsonReader)
        {
            var type = GetDeserializeType<T>();
            var obj = (IApiResponse<T>?)jsonSerializer.Deserialize(jsonReader, type);
            return obj ?? ClientDeserializeFail<T>();
        }

        public static async ValueTask<IApiResponse<T>> DeserializeAsync<T>(
            Stream stream,
            CancellationToken cancellationToken)
        {
            var type = GetDeserializeType<T>();
            var obj = await MessagePackSerializer.DeserializeAsync(
                type, stream, options: Serializable.lz4Options, cancellationToken: cancellationToken);
            return (IApiResponse<T>?)obj ?? ClientDeserializeFail<T>();
        }

        public static IApiResponse<T> Deserialize<T>(byte[] buffer)
        {
            var type = GetDeserializeType<T>();
            var obj = MessagePackSerializer.Deserialize(type, buffer, options: Serializable.lz4Options);
            return (IApiResponse<T>?)obj ?? ClientDeserializeFail<T>();
        }

        public static bool TryGetContent<T>(
             this IApiResponse<T> response,
             [NotNullWhen(true)] out T? content)
        {
            content = response.Content;
            return response.IsSuccess && content != null;
        }

        static bool IsClientExceptionOrServerException(ApiResponseCode code) => code switch
        {
            ApiResponseCode.ClientException => true,
            _ => false,
        };

        public static string GetMessage(this ApiResponseCode code, string? errorAppendText = null, string? errorFormat = null)
        {
            if (code == ApiResponseCode.OK)
            {
                return string.Empty;
            }
            else if (code == ApiResponseCode.Unauthorized)
            {
                return SR.ApiResponseCode_Unauthorized;
            }
            else if (code == ApiResponseCode.IsNotOfficialChannelPackage)
            {
                return SR.IsNotOfficialChannelPackageWarning;
            }
            else if (code == ApiResponseCode.AppObsolete)
            {
                return SR.ApiResponseCode_AppObsolete;
            }
            else if (code == ApiResponseCode.UserIsBan)
            {
                return ModelValidatorErrorDescriber.UserIsBanErrorMessage;
            }
            string message;
            var notErrorAppendText = string.IsNullOrWhiteSpace(errorAppendText);
            if (string.IsNullOrWhiteSpace(errorFormat))
            {
                if (notErrorAppendText)
                {
                    errorFormat = IsClientExceptionOrServerException(code) ? SR.ClientError_ : SR.ServerError_;
                }
                else
                {
                    errorFormat = IsClientExceptionOrServerException(code) ? SR.ClientError__ : SR.ServerError__;
                }
            }
            if (notErrorAppendText)
            {
                message = errorFormat.Format((int)code);
            }
            else
            {
                message = errorFormat.Format((int)code, errorAppendText);
            }
            return message;
        }

        internal static string GetMessage(this IApiResponse response, string? errorAppendText = null, string? errorFormat = null)
        {
            var message = response.InternalMessage;
            if (string.IsNullOrWhiteSpace(message))
            {
                message = GetMessage(response.Code, errorAppendText, errorFormat);
            }
            return message;
        }

        public static IApiResponse Code(ApiResponseCode code, string? message = null, Exception? exception = null) => new ApiResponseImpl
        {
            Code = code,
            InternalMessage = message,
            ClientException = exception,
        };

        static readonly Lazy<IApiResponse> okRsp = new(() => Code(ApiResponseCode.OK));

        public static IApiResponse Ok() => okRsp.Value;

        public static IApiResponse<T> Code<T>(
            ApiResponseCode code,
            string? message,
            T? content,
            Exception? exception = null) => new ApiResponseImpl<T>
            {
                Code = code,
                InternalMessage = message,
                Content = content,
                ClientException = exception,
            };

        public static IApiResponse<T> Code<T>(
            ApiResponseCode code,
            string? message = null) => Code<T>(code, message, default);

        public static IApiResponse<T> Ok<T>(T? content = default)
            => Code(ApiResponseCode.OK, null, content);

        public static IApiResponse<T> Fail<T>(string? message = null)
            => Code<T>(ApiResponseCode.Fail, message);

        public static IApiResponse Fail(string? message = null)
            => Code(ApiResponseCode.Fail, message);

        public static IApiResponse Exception(Exception exception)
            => Code(ApiResponseCode.ClientException, null, exception);

        public static IApiResponse<T> Exception<T>(Exception exception)
            => Code<T>(ApiResponseCode.ClientException, null, default, exception);
    }
}