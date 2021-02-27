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

        public string? LicenseText { get; set; }

        const string hr = "------------------------------";

        static StringBuilder ToString(OpenSourceLibrary m, StringBuilder sb)
        {
            sb.AppendLine(hr);
            sb.AppendLine(m.Name);
            sb.AppendLine(hr);
            sb.AppendLine(m.Url);
            sb.AppendLine();
            sb.AppendLine(m.LicenseText);
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