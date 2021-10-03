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
    public abstract class ApiResponseImplBase : IApiResponse
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

    [MPObject]
    public sealed class ApiResponseImpl<T> : ApiResponseImplBase, IApiResponse<T>
    {
        [MPKey(LastMKeyIndex + 1)]
        [NJsonProperty("🦓")]
        [SJsonProperty("🦓")]
        public T? Content { get; set; }
    }

    [MPObject]
    public sealed class ApiResponseImpl : ApiResponseImplBase, IApiResponse<object>
    {
        [IgnoreDataMember]
        [MPIgnore]
        [SJSONIgnore]
        [NJSONIgnore]
        public object? Content => null;
    }
}