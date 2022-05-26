namespace System.IO;

public interface IFileStreamWrapper
{
    /// <inheritdoc cref="FileStream.Name"/>
    string? Name { get; }

    FileStream? FileStream => IOPath.OpenRead(Name);

    /// <inheritdoc cref="FileStream(string, FileMode)"/>
    FileStream GetFileStream(FileMode mode) => new(Name, mode);

    /// <inheritdoc cref="FileStream(string, FileMode, FileAccess)"/>
    FileStream GetFileStream(FileMode mode, FileAccess access) => new(Name, mode, access);

    /// <inheritdoc cref="FileStream(string, FileMode, FileAccess, FileShare)"/>
    FileStream GetFileStream(FileMode mode, FileAccess access, FileShare share) => new(Name, mode, access, share);

    /// <inheritdoc cref="FileStream(string, FileMode, FileAccess, FileShare, int)"/>
    FileStream GetFileStream(FileMode mode, FileAccess access, FileShare share, int bufferSize) => new(Name, mode, access, share, bufferSize);

    /// <inheritdoc cref="FileStream(string, FileMode, FileAccess, FileShare, int, FileOptions)"/>
    FileStream GetFileStream(FileMode mode, FileAccess access, FileShare share, int bufferSize, FileOptions options) => new(Name, mode, access, share, bufferSize, options);
}
