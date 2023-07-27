// ReSharper disable once CheckNamespace
namespace BD.WTTS.Services;

partial interface IPCPlatformService
{
    /// <inheritdoc cref="IOPath.RegexSearchFile(string, string)"/>
    string RegexSearchFile(string filePath, string pattern);

    /// <inheritdoc cref="IOPath.RegexSearchFolder(string, string, string)"/>
    string RegexSearchFolder(string dirPath, string pattern, string wildcard = "");
}