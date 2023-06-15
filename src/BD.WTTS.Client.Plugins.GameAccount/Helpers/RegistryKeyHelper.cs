namespace BD.WTTS;

public static class RegistryKeyHelper
{
    public static string ByteArrayToString(byte[] ba) => BitConverter.ToString(ba).Replace("-", "");

    public static byte[] StringToByteArray(string hex)
    {
        var numberChars = hex.Length;
        var bytes = new byte[numberChars / 2];
        for (var i = 0; i < numberChars; i += 2)
            bytes[i / 2] = Convert.ToByte(hex.Substring(i, 2), 16);
        return bytes;
    }

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

    /// <summary>
    /// Sets the value of a Registry key (Requires special path)
    /// </summary>
    /// <param name="encodedPath">HKXX\\path:subkey</param>
    /// <param name="value">Value, or empty to "clear"</param>
    [SupportedOSPlatform("windows")]
    public static bool SetRegistryKey(string encodedPath, string value = "")
    {
        var (rootKey, path, subKey) = ExplodeRegistryKey(encodedPath);

        try
        {
            using var key = rootKey.CreateSubKey(path);
            if (value.StartsWith("(hex)"))
            {
                value = value[6..];
                key?.SetValue(subKey, StringToByteArray(value));
            }
            else
                key?.SetValue(subKey, value);
        }
        catch (Exception e)
        {
            Log.Error(nameof(RegistryKeyHelper), e, "SetRegistryKey failed");
            return false;
        }

        return true;
    }

    [SupportedOSPlatform("windows")]
    public static bool DeleteRegistryKey(string encodedPath)
    {
        var (rootKey, path, subKey) = ExplodeRegistryKey(encodedPath);

        try
        {
            using var key = rootKey.OpenSubKey(path, true);
            key?.DeleteValue(subKey);
        }
        catch (Exception e)
        {
            Log.Error(nameof(RegistryKeyHelper), e, "DeleteRegistryKey failed");
            return false;
        }

        return true;
    }
}
