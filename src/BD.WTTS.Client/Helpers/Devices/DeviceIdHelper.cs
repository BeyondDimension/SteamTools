// ReSharper disable once CheckNamespace
namespace BD.WTTS;

public static class DeviceIdHelper
{
    const int DeviceIdRLength = Constants.Lengths.DeviceIdR;

    public const int MaxLength = Constants.Lengths.Max_DeviceId;

    static Guid GetGuid(string key)
    {
        var value = Preferences2.Get(key, null);
        if (!(!string.IsNullOrWhiteSpace(value) && ShortGuid.TryParse(value, out Guid guid)))
        {
            guid = Guid.NewGuid();
            Preferences2.Set(key, guid.ToStringS());
        }
        return guid;
    }

    static string Get(string key)
    {
        var value = Preferences2.Get(key, null);
        if (string.IsNullOrWhiteSpace(value))
        {
            value = Random2.GenerateRandomString(DeviceIdRLength, String2.DigitsLetters);
            Preferences2.Set(key, value);
        }
        return value;
    }

    internal static Guid DeviceIdG => GetGuid("KEY_DEVICE_ID_G");

    internal static string DeviceIdR => Get("KEY_DEVICE_ID_R");

    internal static string DeviceIdN
    {
        get
        {
            var s = IPlatformService.Instance;
            (var key, var iv) = s.MachineSecretKey;
            var data = key.FirstOrDefault() % 2 == 0 ? key.Concat(iv) : iv.Concat(key);
            return Hashs.String.SHA256(data.ToArray(), isLower: false);
        }
    }
}