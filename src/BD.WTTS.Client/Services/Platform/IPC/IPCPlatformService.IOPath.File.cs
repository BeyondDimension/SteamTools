// ReSharper disable once CheckNamespace
namespace BD.WTTS.Services;

partial interface IPCPlatformService
{
    /// <inheritdoc cref="IOPath.CopyFile(string, string, bool)"/>
    void CopyFile(string source, string dest, bool overwrite = true);

    /// <inheritdoc cref="IOPath.CopyFilesRecursive(string?, string, bool)"/>
    bool CopyFilesRecursive(string? inputFolder, string outputFolder, bool overwrite = true);

    /// <inheritdoc cref="IOPath.ReadAllText(string, Encoding?)"/>
    string ReadAllText(string path, int? encoding = null);

    /// <inheritdoc cref="IOPath.ReadAllTextAsync(string, Encoding?, CancellationToken)"/>
    Task<string> ReadAllTextAsync(string path, int? encoding, CancellationToken cancellationToken = default);

    /// <inheritdoc cref="IOPath.FileTryDelete(string)"/>
    bool FileTryDelete(string filePath);

    #region WriteAll

    /// <inheritdoc cref="File.WriteAllText(string, string?, Encoding)"/>
    bool WriteAllText(string path, string? contents, int? encoding = null);

    /// <inheritdoc cref="File.WriteAllTextAsync(string, string?, Encoding, CancellationToken)"/>
    Task<bool> WriteAllTextAsync(string path, string? contents, int? encoding = null, CancellationToken cancellationToken = default);

    /// <inheritdoc cref="File.WriteAllLines(string, IEnumerable{string}, Encoding)"/>
    bool WriteAllLines(string path, IEnumerable<string> contents, int? encoding = null);

    /// <inheritdoc cref="File.WriteAllLinesAsync(string, IEnumerable{string}, Encoding, CancellationToken)"/>
    Task<bool> WriteAllLinesAsync(string path, IEnumerable<string> contents, int? encoding = null, CancellationToken cancellationToken = default);

    /// <inheritdoc cref="File.WriteAllBytes(string, byte[])"/>
    bool WriteAllBytes(string path, byte[] bytes);

    /// <inheritdoc cref="File.WriteAllBytesAsync(string, byte[], CancellationToken)"/>
    Task<bool> WriteAllBytesAsync(string path, byte[] bytes, CancellationToken cancellationToken = default);

    #endregion
}

public static partial class IPCPlatformServiceExtensions
{
    /// <inheritdoc cref="IOPath.ReadAllText(string, Encoding?)"/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static string ReadAllText(
        this IPCPlatformService s,
        string path, Encoding? encoding = null)
    {
        int? encoding_ = encoding?.CodePage;
        return s.ReadAllText(path, encoding_);
    }

    /// <inheritdoc cref="IOPath.ReadAllTextAsync(string, Encoding?, CancellationToken)"/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Task<string> ReadAllTextAsync(
        this IPCPlatformService s,
        string path, Encoding? encoding, CancellationToken cancellationToken = default)
    {
        int? encoding_ = encoding?.CodePage;
        return s.ReadAllTextAsync(path, encoding_, cancellationToken);
    }

    /// <inheritdoc cref="IOPath.ReadAllTextAsync(string, Encoding?, CancellationToken)"/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Task<string> ReadAllTextAsync(
        this IPCPlatformService s,
        string path, CancellationToken cancellationToken = default)
    {
        return s.ReadAllTextAsync(path, null, cancellationToken);
    }
}