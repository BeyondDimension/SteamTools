namespace BD.WTTS;

public static class RegistryKeyHelper
{
    [SupportedOSPlatform("windows")]
    public static RegistryKey? ExpandRegistryAbbreviation(string abv)
    {
        return abv switch
        {
            "HKCR" => Registry.ClassesRoot,
            "HKCU" => Registry.CurrentUser,
            "HKLM" => Registry.LocalMachine,
            "HKCC" => Registry.CurrentConfig,
            "HKPD" => Registry.PerformanceData,
            _ => null
        };
    }

    /// <summary>
    /// Break an encoded registry key into it's separate parts
    /// </summary>
    /// <param name="encodedPath">HKXX\\path:SubKey</param>
    [SupportedOSPlatform("windows")]
    private static (RegistryKey, string, string) ExplodeRegistryKey(string encodedPath)
    {
        var rootKey = ExpandRegistryAbbreviation(encodedPath[..4]); // Get HKXX
        encodedPath = encodedPath[5..]; // Remove HKXX\\
        var path = encodedPath.Split(":")[0];
        var subKey = encodedPath.Split(":")[1];

        return (rootKey, path, subKey);
    }

    /// <summary>
    /// Read the value of a Registry key (Requires special path)
    /// </summary>
    /// <param name="encodedPath">HKXX\\path:SubKey</param>
    [SupportedOSPlatform("windows")]
    public static object? ReadRegistryKey(string encodedPath)
    {
        var (rootKey, path, subKey) = ExplodeRegistryKey(encodedPath);

        try
        {
            return rootKey.OpenSubKey(path)?.GetValue(subKey);
        }
        catch (Exception e)
        {
            Log.Error(nameof(RegistryKeyHelper), e, "ReadRegistryKey failed");
            return null;
        }
    }

    /// <summary>
    /// Read the value of a Registry key (Requires special path)
    /// </summary>
    /// <param name="encodedPath">HKXX\\path:SubKey</param>
    [SupportedOSPlatform("windows")]
    public static bool TryReadRegistryKey(string encodedPath, out object? value)
    {
        value = ReadRegistryKey(encodedPath);
        return value != null;
    }
}
