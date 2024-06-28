// ReSharper disable once CheckNamespace
namespace BD.WTTS.Services;

partial interface IPCPlatformService
{
    /// <inheritdoc cref="IOPath.CopyFile(string, string, bool)"/>
    [Mobius(Obsolete = true)]
    void CopyFile(string source, string dest, bool overwrite = true);

    /// <inheritdoc cref="IOPath.CopyFilesRecursive(string?, string, bool)"/>
    [Mobius(Obsolete = true)]
    bool CopyFilesRecursive(string? inputFolder, string outputFolder, bool overwrite = true);

    /// <inheritdoc cref="IOPath.ReadAllText(string, Encoding?)"/>
    [Mobius(Obsolete = true)]
    string ReadAllText(string path, int? encoding = null);

    /// <inheritdoc cref="IOPath.ReadAllTextAsync(string, Encoding?, CancellationToken)"/>
    [Mobius(Obsolete = true)]
    Task<string> ReadAllTextAsync(string path, int? encoding, CancellationToken cancellationToken = default);

    /// <inheritdoc cref="IOPath.FileTryDelete(string)"/>
    [Mobius(Obsolete = true)]
    bool FileTryDelete(string filePath);

    #region WriteAll

    /// <inheritdoc cref="File.WriteAllText(string, string?, Encoding)"/>
    [Mobius(Obsolete = true)]
    bool WriteAllText(string path, string? contents, int? encoding = null);

    /// <inheritdoc cref="File.WriteAllTextAsync(string, string?, Encoding, CancellationToken)"/>
    [Mobius(Obsolete = true)]
    Task<bool> WriteAllTextAsync(string path, string? contents, int? encoding = null, CancellationToken cancellationToken = default);

    /// <inheritdoc cref="File.WriteAllLines(string, IEnumerable{string}, Encoding)"/>
    [Mobius(Obsolete = true)]
    bool WriteAllLines(string path, IEnumerable<string> contents, int? encoding = null);

    /// <inheritdoc cref="File.WriteAllLinesAsync(string, IEnumerable{string}, Encoding, CancellationToken)"/>
    [Mobius(Obsolete = true)]
    Task<bool> WriteAllLinesAsync(string path, IEnumerable<string> contents, int? encoding = null, CancellationToken cancellationToken = default);

    /// <inheritdoc cref="File.WriteAllBytes(string, byte[])"/>
    [Mobius(Obsolete = true)]
    bool WriteAllBytes(string path, byte[] bytes);

    /// <inheritdoc cref="File.WriteAllBytesAsync(string, byte[], CancellationToken)"/>
    [Mobius(Obsolete = true)]
    Task<bool> WriteAllBytesAsync(string path, byte[] bytes, CancellationToken cancellationToken = default);

    #endregion
}

public static partial class IPCPlatformServiceExtensions
{
    /// <inheritdoc cref="IOPath.ReadAllText(string, Encoding?)"/>
    [Mobius(Obsolete = true)]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static string ReadAllText(
        this IPCPlatformService s,
        string path, Encoding? encoding = null)
    {
        int? encoding_ = encoding?.CodePage;
        return s.ReadAllText(path, encoding_);
    }

    /// <inheritdoc cref="IOPath.ReadAllTextAsync(string, Encoding?, CancellationToken)"/>
    [Mobius(Obsolete = true)]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Task<string> ReadAllTextAsync(
        this IPCPlatformService s,
        string path, Encoding? encoding, CancellationToken cancellationToken = default)
    {
        int? encoding_ = encoding?.CodePage;
        return s.ReadAllTextAsync(path, encoding_, cancellationToken);
    }

    /// <inheritdoc cref="IOPath.ReadAllTextAsync(string, Encoding?, CancellationToken)"/>
    [Mobius(Obsolete = true)]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Task<string> ReadAllTextAsync(
        this IPCPlatformService s,
        string path, CancellationToken cancellationToken = default)
    {
        return s.ReadAllTextAsync(path, null, cancellationToken);
    }
}