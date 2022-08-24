using NUnit.Framework;
using System.Security.Cryptography;
using System.Text;
#if WINDOWS
using Windows.Security.Cryptography.DataProtection;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Runtime.Versioning;
#endif

namespace System;

[TestFixture]
public class ByteArrayTest
{
    [Test]
    public void ByteArray()
    {
        var values = new byte[] { 0xff };
        var sbytes = values.ToSByteArray();
        var bytes = sbytes.ToByteArray();
        Assert.IsTrue(bytes[0] == values[0]);
    }

    static CipherMode CFB => OperatingSystem2.IsAndroid() ? CipherMode.CBC : CipherMode.CFB;

    [Test]
    public void MultipleEncrypt()
    {
        var aes_cbc_1 = AESUtils.Create();
        var aes_cfb_1 = AESUtils.Create(mode: CFB);
        var aes_cbc_2 = AESUtils.Create();

        var value = DateTime.Now.ToString() + " 🔮🎮";

        var bytes_1 = AESUtils.EncryptToByteArray(aes_cbc_1, value);
        var bytes_2 = AESUtils.Encrypt(aes_cfb_1, bytes_1);
        var bytes_3 = AESUtils.Encrypt(aes_cbc_2, bytes_2);
        var bytes_4 = bytes_3;

        var d_bytes_4 = bytes_4;

#pragma warning disable CA1416 // 验证平台兼容性
#if !ANDROID && !__ANDROID__ && !__MOBILE__
        if (OperatingSystem2.IsWindows())
        {
            bytes_4 = ProtectedData.Protect(bytes_3, null, DataProtectionScope.LocalMachine);

            d_bytes_4 = ProtectedData.Unprotect(bytes_4, null, DataProtectionScope.LocalMachine);
        }
#endif
#pragma warning restore CA1416 // 验证平台兼容性

        var d_bytes_3 = AESUtils.Decrypt(aes_cbc_2, d_bytes_4);
        var d_bytes_2 = AESUtils.Decrypt(aes_cfb_1, d_bytes_3);
        var d_value = AESUtils.DecryptToString(aes_cbc_1, d_bytes_2);

        TestContext.WriteLine(d_value);
    }

    [Test]
    public void LocalEncrypt()
    {
        var value = DateTime.Now.ToString() + " 🔮🎮";

        var key_str = Environment.MachineName;

        (var key, var iv) = AESUtils.GetParameters(key_str);

        var aes_cfb = AESUtils.Create(mode: CFB);
        aes_cfb.Key = key;
        aes_cfb.IV = iv;

        var data = AESUtils.EncryptToByteArray(aes_cfb, value);

        var value1 = AESUtils.DecryptToString(aes_cfb, data);

        Assert.IsTrue(value == value1);

        var aes_cfb_1 = AESUtils.Create(mode: CFB);
        aes_cfb_1.Key = key;
        aes_cfb_1.IV = iv;

        var value2 = AESUtils.DecryptToString(aes_cfb, data);
        var value3 = AESUtils.DecryptToString(aes_cfb_1, data);

        Assert.IsTrue(value2 == value3);

        Assert.IsTrue(value1 == value3);
    }

#if WINDOWS
    /// <summary>
    /// Windows10DataProtectionProvider 的单元测试
    /// </summary>
    /// <returns></returns>
    [Test]
    public async Task DataProtection()
    {
        if (!OperatingSystem.IsWindowsVersionAtLeast(10, 0, 10240)) return;

        var value = DateTime.Now.ToString() + " 🔮🎮";
        var bytes = Encoding.UTF8.GetBytes(value);

        var e = await ProtectCore2Async(bytes);
        var d = await UnprotectCore2Async(e);

        TestContext.WriteLine(Encoding.UTF8.GetString(d));

        Assert.IsTrue(d.SequenceEqual(bytes));
    }
#endif

#if WINDOWS
    [SupportedOSPlatform("Windows10.0.10240.0")]
    static DataProtectionProvider GetDataProtectionProvider(string? protectionDescriptor = null)
    {
        DataProtectionProvider provider = protectionDescriptor == null ? new() : new(protectionDescriptor);
        return provider;
    }

    [SupportedOSPlatform("Windows10.0.10240.0")]
    static async Task<byte[]> ProtectCoreAsync(byte[] data)
    {
        // LOCAL=user and LOCAL=machine do not require enterprise auth capability
        var provider = GetDataProtectionProvider("LOCAL=user");

        // https://appcenter.ms/orgs/BeyondDimension/apps/Steam/crashes/errors/842356268u/overview
        // System.Runtime.InteropServices.COMException: 无法在设置线程模式后对其加以更改。 (0x80010106 (RPC_E_CHANGED_MODE))

        var buffer = await provider.ProtectAsync(data.AsBuffer());

        var encBytes = buffer.ToArray();

        return encBytes;
    }

    [SupportedOSPlatform("Windows10.0.10240.0")]
    static async Task<byte[]> ProtectCore2Async(byte[] data) => await await Task.Factory.StartNew(async () => await ProtectCoreAsync(data));

    [SupportedOSPlatform("Windows10.0.10240.0")]
    static async Task<byte[]> UnprotectCoreAsync(byte[] data)
    {
        var provider = GetDataProtectionProvider();

        var buffer = await provider.UnprotectAsync(data.AsBuffer());

        return buffer.ToArray();
    }

    [SupportedOSPlatform("Windows10.0.10240.0")]
    static async Task<byte[]> UnprotectCore2Async(byte[] data) => await await Task.Factory.StartNew(async () => await UnprotectCoreAsync(data));
#endif
}