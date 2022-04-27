namespace System.IO
{
    public interface IFileStreamWrapper
    {
        /// <inheritdoc cref="FileStream.Name"/>
        string Name { get; }

        /// <inheritdoc cref="FileStream(string, FileMode)"/>
        FileStream GetFileStream(FileMode mode);

        /// <inheritdoc cref="FileStream(string, FileMode, FileAccess)"/>
        FileStream GetFileStream(FileMode mode, FileAccess access);

        /// <inheritdoc cref="FileStream(string, FileMode, FileAccess, FileShare)"/>
        FileStream GetFileStream(FileMode mode, FileAccess access, FileShare share);

        /// <inheritdoc cref="FileStream(string, FileMode, FileAccess, FileShare, int)"/>
        FileStream GetFileStream(FileMode mode, FileAccess access, FileShare share, int bufferSize);

        /// <inheritdoc cref="FileStream(string, FileMode, FileAccess, FileShare, int, FileOptions)"/>
        FileStream GetFileStream(FileMode mode, FileAccess access, FileShare share, int bufferSize, FileOptions options);
    }
}
