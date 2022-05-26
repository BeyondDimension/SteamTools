//namespace System.IO.FileFormats.Internals;

//partial class ImageFileFormat
//{
//    /// <summary>
//    /// Animated Portable Network Graphics
//    /// <para>https://en.wikipedia.org/wiki/APNG</para>
//    /// </summary>
//    public static class APNG
//    {
//        public const ImageFormat Format = ImageFormat.APNG;

//        public const string DefaultFileExtension = FileEx.APNG;

//        public const string DefaultMIME = MediaTypeNames.APNG;

//        public static readonly byte?[] MagicNumber;

//        static APNG()
//        {
//            MagicNumber = new byte?[] {
//                0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A, null, null, null, null, null, null, null, null,
//                null, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null,
//                null, null, null, null, 0x08, 0x61, 0x63, 0x54, 0x4C,
//            };
//        }
//    }
//}