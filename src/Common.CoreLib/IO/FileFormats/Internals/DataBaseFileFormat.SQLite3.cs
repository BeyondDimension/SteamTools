namespace System.IO.FileFormats.Internals;

partial class DataBaseFileFormat
{
    public static class SQLite3
    {
        public static readonly byte[] MagicNumber = new byte[16] { 83, 81, 76, 105, 116, 101, 32, 102, 111, 114, 109, 97, 116, 32, 51, 0 };
    }
}