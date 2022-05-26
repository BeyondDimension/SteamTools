using FE = System.FileEx;

namespace System.IO.FileFormats.Internals;

partial class ImageFileFormat
{
    /// <summary>
    /// Windows Bitmap
    /// <para>https://en.wikipedia.org/wiki/BMP_file_format</para>
    /// </summary>
    public static class BMP
    {
        public const ImageFormat Format = ImageFormat.BMP;

        public const string DefaultFileExtension = FE.BMP;

        public const string DefaultMIME = MediaTypeNames.BMP;

        public static readonly string[] FileExtensions = { DefaultFileExtension, ".dib" };

        public static readonly string[] MIME = { DefaultMIME, "image/x-bmp" };

        public static readonly byte[] MagicNumber;

        static BMP()
        {
            MagicNumber = new byte[] { 0x42, 0x4D };
        }
    }
}