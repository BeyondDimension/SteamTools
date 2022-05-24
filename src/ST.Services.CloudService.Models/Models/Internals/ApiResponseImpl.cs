using System.Application.Models.Internals;
using System.Collections.Generic;
using System.Runtime.Serialization;
using MPIgnore = MessagePack.IgnoreMemberAttribute;
using MPKey = MessagePack.KeyAttribute;
using MPObject = MessagePack.MessagePackObjectAttribute;
using NJSONIgnore = Newtonsoft.Json.JsonIgnoreAttribute;
using NJsonProperty = Newtonsoft.Json.JsonPropertyAttribute;
using SJSONIgnore = System.Text.Json.Serialization.JsonIgnoreAttribute;
using SJsonProperty = System.Text.Json.Serialization.JsonPropertyNameAttribute;

namespace System.Application.Models.Internals
{
    public abstract class ApiResponseImpl : IApiResponse
    {
        ApiResponseCode mCode;
        bool mIsSuccess;

        [MPKey(0)]
        [NJsonProperty("🦄")]
        [SJsonProperty("🦄")]
        public ApiResponseCode Code
        {
            get => mCode;
            set
            {
                mCode = value;
                // https://github.com/dotnet/corefx/blob/v3.1.6/src/System.Net.Http/src/System/Net/Http/HttpResponseMessage.cs#L143
                var code = (int)mCode;
                mIsSuccess = code >= 200 && code <= 299;
            }
        }

        [MPKey(LastMKeyIndex)]
        [NJsonProperty("🐴")]
        [SJsonProperty("🐴")]
        public string? InternalMessage { get; set; }

        string? IApiResponse.InternalMessage
        {
            get => InternalMessage;
            set => InternalMessage = value;
        }

        /// <summary>
        /// 最后一个 MessagePack 序列化 下标，继承自此类，新增需要序列化的字段/属性，标记此值+1，+2
        /// </summary>
        protected const int LastMKeyIndex = 1;

        [IgnoreDataMember]
        [MPIgnore]
        [SJSONIgnore]
        [NJSONIgnore]
        public bool IsSuccess => mIsSuccess;

        [IgnoreDataMember]
        [MPIgnore]
        [SJSONIgnore]
        [NJSONIgnore]
        public Exception? ClientException { get; set; }

        [IgnoreDataMember]
        [MPIgnore]
        [SJSONIgnore]
        [NJSONIgnore]
        public string? Url { get; set; }
    }
}

namespace System.Application.Models
{
    [MPObject]
    public sealed class ApiRsp<T> : ApiResponseImpl, IApiResponse<T>
    {
        [MPKey(LastMKeyIndex + 1)]
        [NJsonProperty("🦓")]
        [SJsonProperty("🦓")]
        public T? Content { get; set; }

        public static implicit operator ApiRsp<T>(T content) => ApiResponse.Ok(content);

        public static implicit operator ApiRsp<T>(ApiResponseCode code) => ApiResponse.Code<T>(code);

        public static implicit operator ApiRsp<T>((ApiResponseCode code, string? message) args) => ApiResponse.Code<T>(args.code, args.message);
    }

    [MPObject]
    public sealed class ApiRsp : ApiResponseImpl, IApiResponse<object>
    {
        [IgnoreDataMember]
        [MPIgnore]
        [SJSONIgnore]
        [NJSONIgnore]
        public object? Content => null;

        public static implicit operator ApiRsp(ApiResponseCode code) => ApiResponse.Code(code);

        public static implicit operator ApiRsp((ApiResponseCode code, string? message) args) => ApiResponse.Code(args.code, args.message);
    }
}