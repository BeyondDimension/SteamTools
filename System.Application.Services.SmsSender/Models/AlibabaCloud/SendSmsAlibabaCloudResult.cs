namespace System.Application.Models.AlibabaCloud
{
    public sealed class SendSmsAlibabaCloudResult : AlibabaCloudResult<SendSmsAlibabaCloudResult>
    {
        public string? Message { get; set; }

        public string? RequestId { get; set; }

        public string? BizId { get; set; }
    }
}