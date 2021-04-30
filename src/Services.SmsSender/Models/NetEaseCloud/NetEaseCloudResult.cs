using N_JsonProperty = Newtonsoft.Json.JsonPropertyAttribute;
using S_JsonProperty = System.Text.Json.Serialization.JsonPropertyNameAttribute;

namespace System.Application.Models.NetEaseCloud
{
    public class NetEaseCloudResult<T> : JsonModel<T>, ISmsSubResult where T : NetEaseCloudResult<T>
    {
        [N_JsonProperty(code)]
        [S_JsonProperty(code)]
        public SendSmsResponseCode Code { get; set; }

        protected const string code = nameof(code);
        protected const string msg = nameof(msg);
        protected const string obj = nameof(obj);

        public virtual bool IsOK() => Code == SendSmsResponseCode.操作成功;

        public virtual bool IsCheckSmsFail() => Code == SendSmsResponseCode.验证失败;

        protected virtual string? GetRecord() => $"code: {Code}";

        string? ISmsSubResult.GetRecord() => GetRecord();
    }

    public sealed class NetEaseCloudResult : NetEaseCloudResult<NetEaseCloudResult>
    {
    }
}