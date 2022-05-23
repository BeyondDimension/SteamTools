using NUnit.Framework;
using System.Security.Cryptography;

namespace System
{
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
    }
}