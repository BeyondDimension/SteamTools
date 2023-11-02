// ReSharper disable once CheckNamespace

namespace BD.WTTS.Services;

partial interface IPlatformService
{
    private static (byte[] key, byte[] iv) GetMachineSecretKey(string? value)
    {
        value ??= string.Empty;
        var result = AESUtils.GetParameters(value);
        return result;
    }

    protected static Lazy<(byte[] key, byte[] iv)> GetMachineSecretKey(Func<string?> action) => new(() =>
    {
        string? value = null;
        try
        {
            value = action();
        }
        catch (Exception e)
        {
            Log.Warn(TAG, e, "GetMachineSecretKey fail.");
        }
        if (string.IsNullOrWhiteSpace(value))
        {
            value = Environment.MachineName;
        }
        return GetMachineSecretKey(value);
    });

    protected static (byte[] key, byte[] iv) GetMachineSecretKeyBySecureStorage()
    {
        if (!CommonEssentials.IsSupported) throw new PlatformNotSupportedException();
        const string KEY_MACHINE_SECRET = "KEY_MACHINE_SECRET_2105";
        var guid = GetMachineSecretKeyGuid();
        static Guid GetMachineSecretKeyGuid()
        {
            var secureStorage = ISecureStorage.Instance;
            Func<Task<string?>> getAsync = () => secureStorage.GetAsync(KEY_MACHINE_SECRET);
            var guidStr = getAsync.RunSync();
            if (Guid.TryParse(guidStr, out var guid)) return guid;
            guid = Guid.NewGuid();
            guidStr = guid.ToString();
            Func<Task> setAsync = () => secureStorage.SetAsync(KEY_MACHINE_SECRET, guidStr);
            setAsync.RunSync();
            return guid;
        }
        var r = AESUtils.GetParameters(guid.ToByteArray());
        return r;
    }

    protected static readonly Lazy<(byte[] key, byte[] iv)> mMachineSecretKeyBySecureStorage = new(GetMachineSecretKeyBySecureStorage);

    (byte[] key, byte[] iv) MachineSecretKey => mMachineSecretKeyBySecureStorage.Value;
}