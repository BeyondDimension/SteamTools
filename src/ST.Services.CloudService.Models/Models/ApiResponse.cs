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

        public static bool TryGetContent<T>(
            this IApiResponse<T> response,
            [NotNullWhen(true)] out T? content)
        {
            content = response.Content;
            return response.IsSuccess && content != null;
        }

        public static string GetMessage(ApiResponseCode code)
        {
            if (code == ApiResponseCode.Unauthorized)
            {
                return SR.ApiResponseCode_Unauthorized;
            }
            else if (code == ApiResponseCode.IsNotOfficialChannelPackage)
            {
                return SR.IsNotOfficialChannelPackageWarning;
            }
            return string.Format(SR.ServerError_, (int)code);
        }

        public static string GetMessage(IApiResponse response)
        {
#pragma warning disable CS0618 // 类型或成员已过时
            var message = response.Message;
#pragma warning restore CS0618 // 类型或成员已过时
            if (string.IsNullOrWhiteSpace(message))
            {
                return GetMessage(response.Code);
            }
            else
            {
                return message;
            }
        }

        public static IApiResponse Code(ApiResponseCode code, string? message = null) => new ApiResponseImpl
        {
            Code = code,
            Message = message,
        };

        static readonly Lazy<IApiResponse> okRsp = new(() => Code(ApiResponseCode.OK));

        public static IApiResponse Ok() => okRsp.Value;

        public static IApiResponse<T> Code<T>(
            ApiResponseCode code,
            string? message = null,
            T? content = default) => new ApiResponseImpl<T>
            {
                Code = code,
                Message = message,
                Content = content,
            };

        public static IApiResponse<T> Ok<T>(T? content = default)
            => Code(ApiResponseCode.OK, content: content);

        public static IApiResponse<T> Fail<T>(string? message = null)
            => Code<T>(ApiResponseCode.Fail, message);

        public static IApiResponse Fail(string? message = null)
            => Code(ApiResponseCode.Fail, message);
    }
}