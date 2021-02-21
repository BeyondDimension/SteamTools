namespace System.IO.FileFormats.Internals
{
    partial class ImageFileFormat
    {
        /// <summary>
        /// Graphics Interchange Format
        /// <para>https://en.wikipedia.org/wiki/GIF</para>
        /// </summary>
        public static class GIF
        {
            public const ImageFormat Format = ImageFormat.GIF;

            public const string DefaultFileExtension = FileEx.GIF;

            public const string DefaultMIME = MediaTypeNames.GIF;

            public static readonly byte[] MagicNumber1;
            public static readonly byte[] MagicNumber2;
            public static readonly byte[]?[] MagicNumber;

            static GIF()
            {
                MagicNumber1 = new byte[] { 0x47, 0x49, 0x46, 0x38, 0x37, 0x61 };
                MagicNumber2 = new byte[] { 0x47, 0x49, 0x46, 0x38, 0x39, 0x61 };
                MagicNumber = new byte[]?[] { MagicNumber1, MagicNumber2 };
            }
        }
    }
}