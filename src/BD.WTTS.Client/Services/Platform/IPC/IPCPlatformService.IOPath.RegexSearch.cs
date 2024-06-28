// ReSharper disable once CheckNamespace
namespace BD.WTTS.Services;

partial interface IPCPlatformService
{
    /// <inheritdoc cref="IOPath.RegexSearchFile(string, string)"/>
    [Mobius(
"""
Mobius.Services.IO.Abstractions.IFileSystemService
""")]
    string RegexSearchFile(string filePath, string pattern);

    /// <inheritdoc cref="IOPath.RegexSearchFolder(string, string, string)"/>
    [Mobius(
"""
Mobius.Services.IO.Abstractions.IFileSystemService
""")]
    string RegexSearchFolder(string dirPath, string pattern, string wildcard = "");
}