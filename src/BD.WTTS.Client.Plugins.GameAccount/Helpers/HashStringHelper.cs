namespace BD.WTTS;

public static class HashStringHelper
{
    public static string GetSha256HashString(string text) => string.IsNullOrEmpty(text)
    ? string.Empty
    : SHA512.Create().ComputeHash(Encoding.UTF8.GetBytes(text))
        .Aggregate("", (current, x) => current + $"{x:x2}");

    public static string GetSha256HashString(byte[] b) => b.Length == 0
        ? string.Empty
        : SHA512.Create().ComputeHash(b).Aggregate("", (current, x) => current + $"{x:x2}");

    public static string GetFileMd5(string filePath)
    {
        using var md5 = MD5.Create();
        using var stream = File.OpenRead(filePath);
        return stream.Length != 0 ? BitConverter.ToString(md5.ComputeHash(stream)).Replace("-", "").ToLowerInvariant() : "0";
    }
}
