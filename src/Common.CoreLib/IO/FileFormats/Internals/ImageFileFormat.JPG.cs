using FE = System.FileEx;

namespace System.IO.FileFormats.Internals;

partial class ImageFileFormat
{
    /// <summary>
    /// Joint Photographic Experts Group
    /// <para>https://en.wikipedia.org/wiki/JPEG</para>
    /// </summary>
    public static class JPG
    {
        public const ImageFormat Format = ImageFormat.JPEG;

        public const string DefaultFileExtension = FE.JPG;

        public const string DefaultMIME = MediaTypeNames.JPG;

        public static readonly string[] FileExtensions = { DefaultFileExtension, ".jpeg" };

        public static readonly byte[] MagicNumber;

        static JPG()
        {
            MagicNumber = new byte[] { 0xFF, 0xD8, 0xFF };
        }
    }
}