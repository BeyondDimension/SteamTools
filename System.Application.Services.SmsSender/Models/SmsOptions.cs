#pragma warning disable IDE1006 // 命名样式

namespace System.Application.Models
{
    public class SmsOptions
    {
        public AlibabaCloud.SmsOptions? AlibabaCloud { get; set; }

        public NetEaseCloud.SmsOptions? NetEaseCloud { get; set; }

        public _21VianetBlueCloud.SmsOptions? _21VianetBlueCloud { get; set; }

        public static string? GetDefaultProviderName(SmsOptions? options)
        {
            if (options != null)
            {
                if (options._21VianetBlueCloud.HasValue()) return nameof(_21VianetBlueCloud);
                if (options.AlibabaCloud.HasValue()) return nameof(AlibabaCloud);
                if (options.NetEaseCloud.HasValue()) return nameof(NetEaseCloud);
            }
            return null;
        }
    }
}
#pragma warning restore IDE1006 // 命名样式