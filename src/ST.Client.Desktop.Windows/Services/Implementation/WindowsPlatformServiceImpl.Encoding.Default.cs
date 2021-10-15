using System.Text;

namespace System.Application.Services.Implementation
{
    partial class WindowsPlatformServiceImpl
    {
        static volatile Encoding? defaultEncoding;

        Encoding IPlatformService.Default
        {
            get
            {
                if (defaultEncoding == null)
                {
                    defaultEncoding = CreateDefaultEncoding();
                }
                return defaultEncoding;
            }
        }

        static Encoding CreateDefaultEncoding()
        {
            // https://referencesource.microsoft.com/#mscorlib/system/text/encoding.cs,da50c22465aa9274
            // https://docs.microsoft.com/zh-cn/dotnet/api/system.text.encoding.default?view=net-5.0
            // .NET Framework 中的默认属性
            // 在 Windows 桌面上的 .NET Framework 中，
            // Default 属性始终获取系统的活动代码页并创建 Encoding 与其对应的对象。
            // 活动代码页可能是 ANSI 代码页，其中包括 ASCII 字符集以及不同于代码页的其他字符。
            // 由于所有 Default 基于 ANSI 代码页的编码都将丢失数据，因此请考虑 Encoding.UTF8 改用编码。
            // 在 U +00 到 U +7F 范围内，UTF-8 通常是相同的，但可以在不丢失的情况下在 ASCII 范围外对字符进行编码。
            int codePage = NativeMethods.GetACP();
            var encoding = CodePagesEncodingProvider.Instance.GetEncoding(codePage);
            return encoding ?? Encoding.Default;
        }
    }
}