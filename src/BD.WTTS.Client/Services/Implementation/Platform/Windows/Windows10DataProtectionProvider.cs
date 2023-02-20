#if WINDOWS
// https://github.com/xamarin/Essentials/blob/1.6.1/Xamarin.Essentials/SecureStorage/SecureStorage.uwp.cs
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Security.Cryptography.DataProtection;
using IDataProtectionProvider = BD.Common.Services.ILocalDataProtectionProvider.IDataProtectionProvider;

// ReSharper disable once CheckNamespace
namespace BD.WTTS.Services.Implementation;

/// <summary>
/// https://docs.microsoft.com/en-us/uwp/api/windows.security.cryptography.dataprotection.dataprotectionprovider?view=winrt-10240
/// </summary>
sealed class Windows10DataProtectionProvider : IDataProtectionProvider
{
    const string TAG = nameof(Windows10DataProtectionProvider);

    static DataProtectionProvider GetDataProtectionProvider(string? protectionDescriptor = null)
    {
        try
        {
            DataProtectionProvider provider = protectionDescriptor == null ? new() : new(protectionDescriptor);
            return provider;
        }
        catch (Exception e)
        {
            Log.Error(TAG, e,
                "DPP ctor fail, desc: {0}, cl: {1}",
                protectionDescriptor,
                string.Join(' ', Environment.GetCommandLineArgs()));
            throw;
        }
    }

    public async Task<byte[]> ProtectAsync(byte[] data)
    {
        try
        {
            return await ProtectCoreAsync(data);
        }
        catch
        {
            return await ProtectCore2Async(data);
        }
    }

    static async Task<byte[]> ProtectCoreAsync(byte[] data)
    {
        // LOCAL=user and LOCAL=machine do not require enterprise auth capability
        var provider = GetDataProtectionProvider("LOCAL=user");

        // https://appcenter.ms/orgs/BeyondDimension/apps/Steam/crashes/errors/842356268u/overview
        // https://appcenter.ms/orgs/BeyondDimension/apps/Steam/crashes/errors/1550045210u/overview
        // System.Runtime.InteropServices.COMException: 无法在设置线程模式后对其加以更改。 (0x80010106 (RPC_E_CHANGED_MODE))

        var buffer = await provider.ProtectAsync(data.AsBuffer());

        var encBytes = buffer.ToArray();

        return encBytes;
    }

    static async Task<byte[]> ProtectCore2Async(byte[] data) => await await Task.Factory.StartNew(async () => await ProtectCoreAsync(data));

    static async Task<byte[]> UnprotectCoreAsync(byte[] data)
    {
        var provider = GetDataProtectionProvider();

        var buffer = await provider.UnprotectAsync(data.AsBuffer());

        return buffer.ToArray();
    }

    static async Task<byte[]> UnprotectCore2Async(byte[] data) => await await Task.Factory.StartNew(async () => await UnprotectCoreAsync(data));

    public async Task<byte[]> UnprotectAsync(byte[] data)
    {
        try
        {
            return await UnprotectCoreAsync(data);
        }
        catch
        {
            return await UnprotectCore2Async(data);
        }
    }
}
#endif