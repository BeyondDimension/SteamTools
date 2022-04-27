namespace System.IO
{
    public sealed class FileStreamWrapper : Stream, IFileStreamWrapper
    {
        public FileStreamWrapper(string path)
        {
            Name = path;
        }

        public string Name { get; set; }

        FileStream IFileStreamWrapper.GetFileStream(FileMode mode) => new(Name, mode);

        FileStream IFileStreamWrapper.GetFileStream(FileMode mode, FileAccess access) => new(Name, mode, access);

        FileStream IFileStreamWrapper.GetFileStream(FileMode mode, FileAccess access, FileShare share) => new(Name, mode, access, share);

        FileStream IFileStreamWrapper.GetFileStream(FileMode mode, FileAccess access, FileShare share, int bufferSize) => new(Name, mode, access, share, bufferSize);

        FileStream IFileStreamWrapper.GetFileStream(FileMode mode, FileAccess access, FileShare share, int bufferSize, FileOptions options) => new(Name, mode, access, share, bufferSize, options);

        public override bool CanRead => throw new NotImplementedException();

        public override bool CanSeek => throw new NotImplementedException();

        public override bool CanWrite => throw new NotImplementedException();

        public override long Length => throw new NotImplementedException();

        public override long Position { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public override void Flush()
        {
            throw new NotImplementedException();
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            throw new NotImplementedException();
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            throw new NotImplementedException();
        }

        public override void SetLength(long value)
        {
            throw new NotImplementedException();
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            throw new NotImplementedException();
        }
    }
}
