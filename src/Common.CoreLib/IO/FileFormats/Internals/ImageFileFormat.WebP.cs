namespace System.IO.FileFormats.Internals;

partial class ImageFileFormat
{
    /// <summary>
    /// WebP is an image format employing both lossy[6] and lossless compression.
    /// <para>https://en.wikipedia.org/wiki/WebP</para>
    /// </summary>
    public static class WebP
    {
        public const ImageFormat Format = ImageFormat.WebP;

        public const string DefaultFileExtension = FileEx.WEBP;

        public const string DefaultMIME = MediaTypeNames.WEBP;

        public static readonly byte?[] MagicNumber;

        static WebP()
        {
            MagicNumber = new byte?[] { 0x52, 0x49, 0x46, 0x46, null, null, null, null, 0x57, 0x45, 0x42, 0x50 };
        }
    }
}