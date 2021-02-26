using MessagePack;

namespace System.Application.Models
{
    [MessagePackObject(keyAsPropertyName: true)]
    public sealed class OpenSourceLibrary
    {
        public string? Name { get; set; }

        public string? Url { get; set; }

        public string? License { get; set; }

        public string? LicenseText { get; set; }
    }

#if DEBUG

    [Obsolete("", true)]
    public static partial class ProductInfo
    {
        [Obsolete("OpenSourceLibrary", true)]
        public class Library
        {
        }
    }

#endif
}