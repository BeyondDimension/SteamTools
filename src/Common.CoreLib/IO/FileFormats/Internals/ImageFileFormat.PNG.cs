namespace System.IO.FileFormats.Internals;

partial class ImageFileFormat
{
    /// <summary>
    /// Portable Network Graphics
    /// <para>https://en.wikipedia.org/wiki/Portable_Network_Graphics</para>
    /// </summary>
    public static class PNG
    {
        public const ImageFormat Format = ImageFormat.PNG;

        public const string DefaultFileExtension = FileEx.PNG;

        public const string DefaultMIME = MediaTypeNames.PNG;

        public static readonly byte[] MagicNumber;

        static PNG()
        {
            MagicNumber = new byte[] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A };
        }
    }
}