using MessagePack;
using System.Collections.Generic;
using System.Text;

namespace System.Application.Models
{
    [MessagePackObject(keyAsPropertyName: true)]
    public sealed partial class OpenSourceLibrary
    {
        public string? Name { get; set; }

        public string? Url { get; set; }

        public string? License { get; set; }

        public string? LicenseUrl { get; set; }

        public string? LicenseText { get; set; }

        public string? Copyright { get; set; }

        const string hr = "-----------------------------------";

        static StringBuilder ToString(OpenSourceLibrary m, StringBuilder sb)
        {
            sb.AppendLine(hr);
            sb.AppendLine(m.Name);
            sb.AppendLine(hr);
            sb.AppendLine(m.Url);

            if (m.Url != null)
            {
                if (corrections.ContainsKey(m.Url))
                {
                    var (license, copyright) = corrections[m.Url];
                    if (!string.IsNullOrWhiteSpace(license)) m.License = license;
                    if (!string.IsNullOrWhiteSpace(copyright)) m.Copyright = copyright;
                }

                if (string.IsNullOrWhiteSpace(m.Copyright) && m.Url.StartsWith("https://github.com", StringComparison.OrdinalIgnoreCase))
                {
                    var urlSplitArray = m.Url.Split('/', StringSplitOptions.RemoveEmptyEntries);
                    if (urlSplitArray.Length >= 3)
                    {
                        m.Copyright = $"Copyright (c) {urlSplitArray[2]}";
                    }
                }
            }

            if (!string.IsNullOrWhiteSpace(m.Copyright) && !string.IsNullOrWhiteSpace(m.License) && !string.IsNullOrWhiteSpace(m.LicenseUrl) && !string.Equals("NOASSERTION", m.License, StringComparison.OrdinalIgnoreCase))
            {
                sb.AppendLine();
                sb.AppendLine(m.Copyright);
                sb.AppendLine();
                sb.AppendFormat("Licensed under the {0};{1}{2}", m.License, Environment.NewLine, m.LicenseUrl);
                sb.AppendLine();
            }
            else if (!string.IsNullOrEmpty(m.LicenseText))
            {
                sb.AppendLine();
                sb.AppendLine(m.LicenseText);
            }

            return sb;
        }

        public static string ToString(IEnumerable<OpenSourceLibrary> items, StringBuilder? sb = null)
        {
            sb ??= new StringBuilder();
            foreach (var item in items)
            {
                ToString(item, sb);
                sb.AppendLine();
            }
            return sb.ToString();
        }

        public override string ToString() => ToString(this, new StringBuilder()).ToString();

        static readonly IReadOnlyDictionary<string, (string license, string copyright)> corrections = new Dictionary<string, (string license, string copyright)>
        {
            // https://api.github.com/licenses
            { "https://github.com/runceel/Livet", ("Zlib", "") },
            { "https://github.com/ninject/Ninject", ("Apache-2.0", "") },
            { "https://github.com/apache/logging-log4net", ("Apache-2.0", "Copyright (c) The Apache Software Foundation") },
            { "https://github.com/JustArchiNET/ArchiSteamFarm", ("Apache-2.0", "") },
            { "https://github.com/winauth/winauth", ("GPL-3.0", "") },
            { "https://github.com/MichaCo/DnsClient.NET", ("Apache-2.0", "") },
            { "https://github.com/neuecc/MessagePack-CSharp", ("MIT", "") },
            { "https://github.com/gfoidl/Base64", ("MIT", "") },
            { "https://github.com/App-vNext/Polly", ("BSD-3-Clause", "") },
            { "https://github.com/icsharpcode/SharpZipLib", ("MIT", "") },
            { "https://github.com/chromiumembedded/cef", ("BSD-licensed", "") },
            { "https://github.com/cefsharp/CefSharp", ("BSD-3-Clause", "") },
            { "https://github.com/xamarin/essentials", ("MIT", "") },
            { "https://github.com/dotnet/efcore", ("Apache-2.0", "Copyright (c) .NET Foundation and Contributors") },
            { "https://github.com/dotnet/aspnetcore", ("Apache-2.0", "Copyright (c) .NET Foundation and Contributors") },
            { "https://github.com/dotnet/runtime", ("MIT", "") },
            { "https://github.com/ant-design-blazor/ant-design-blazor", ("MIT", "") },
            { "https://github.com/microsoft/appcenter-sdk-dotnet", ("MIT", "") },
            { "https://github.com/nor0x/AppCenter-XMac", ("MIT", "") },
            { "https://github.com/moq/moq4", ("BSD-3-Clause", "") },
            { "https://github.com/xamarin/Xamarin.Forms", ("MIT", "") },
            { "https://github.com/novotnyllc/bc-csharp", ("MIT", "") },
            //{ "", ("", "") },
        };
    }

#if DEBUG

    [Obsolete("待定", true)]
    public static partial class ProductInfo
    {
        [Obsolete("use System.Application.Models.OpenSourceLibrary", true)]
        public class Library
        {
        }

        [Obsolete("use System.Application.Models.OpenSourceLibrary.Values/StringValues", true)]
        public static IReadOnlyCollection<Library> Libraries => throw new NotImplementedException();
    }

#endif
}