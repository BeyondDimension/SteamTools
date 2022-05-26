using N_JsonProperty = Newtonsoft.Json.JsonPropertyAttribute;
using S_JsonProperty = System.Text.Json.Serialization.JsonPropertyNameAttribute;

namespace System.Application.Models.NetEaseCloud;

public sealed class SendSmsNetEaseCloudResult : NetEaseCloudResult<SendSmsNetEaseCloudResult>
{
    [N_JsonProperty(msg)]
    [S_JsonProperty(msg)]
    public string? Msg { get; set; }

    [N_JsonProperty(obj)]
    [S_JsonProperty(obj)]
    public string? Obj { get; set; }

    protected override string? GetRecord() => $"code: {Code}, msg: {Msg}, obj: {Obj}";
}