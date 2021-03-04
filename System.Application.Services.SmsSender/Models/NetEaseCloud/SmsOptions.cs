namespace System.Application.Models.NetEaseCloud
{
    public class SmsOptions
    {
        /// <summary>
        /// 开发者平台分配的appkey
        /// </summary>
        public string? AppKey { get; set; }

        /// <summary>
        /// 开发者平台分配的appSecret
        /// </summary>
        public string? AppSecret { get; set; }

        /// <summary>
        /// 开发者平台分配的模板标志
        /// </summary>
        public SmsOptionsTemplateId<int?>[]? Templates { get; set; }

        public bool IsValid()
        {
            return !string.IsNullOrWhiteSpace(AppKey) &&
                !string.IsNullOrWhiteSpace(AppSecret);
        }
    }
}