using System.Security.Cryptography;

namespace System.Application.Services.Implementation;

/// <inheritdoc cref="ISecondaryPasswordDataProtectionProvider"/>
public class SecondaryPasswordDataProtectionProvider : ISecondaryPasswordDataProtectionProvider
{
    protected readonly SecondaryPasswordDataProtectionType defaultESecondaryPasswordDataProtectionType;

    public SecondaryPasswordDataProtectionProvider()
    {
        defaultESecondaryPasswordDataProtectionType = SecondaryPasswordDataProtectionType.AesCBC;
    }

    protected enum SecondaryPasswordDataProtectionType
    {
        AesCBC,
    }

    byte[] Concat(byte[] value)
    {
        var r = BitConverter.GetBytes((int)defaultESecondaryPasswordDataProtectionType).Concat(value).ToArray();
        return r;
    }

    byte[] E___(Aes aes, byte[] value)
    {
        var r = AESUtils.Encrypt(aes, value);
        return r;
    }

    public byte[]? EB(byte[]? value, string secondaryPassword)
    {
        if (string.IsNullOrEmpty(secondaryPassword))
            throw new ArgumentNullException(nameof(secondaryPassword));

        if (value == null) return value;
        if (value.Length == 0) return value;

        return defaultESecondaryPasswordDataProtectionType switch
        {
            SecondaryPasswordDataProtectionType.AesCBC => Type_0(),
            _ => null,
        };

        byte[]? Type_0()
        {
            (byte[] key, byte[] iv) = AESUtils.GetParameters(secondaryPassword);
            using var aes = AESUtils.Create(key, iv, CipherMode.CBC, PaddingMode.PKCS7);

            var value_1 = E___(aes, value);
            var value_1_r = Concat(value_1);
            return value_1_r;
        }
    }

    byte[]? D___(Aes aes, byte[] value)
    {
        using var transform = aes.CreateDecryptor();
        var data_r = transform.TransformFinalBlock(value, sizeof(int), value.Length - sizeof(int));
        return data_r;
    }

    public byte[]? DB(byte[]? value, string secondaryPassword)
    {
        if (string.IsNullOrEmpty(secondaryPassword))
            throw new ArgumentNullException(nameof(secondaryPassword));

        if (value == null) return value;
        if (value.Length == 0) return value;
        if (value.Length <= sizeof(int)) return null;

        var d_type = (SecondaryPasswordDataProtectionType)BitConverter.ToInt32(value, 0);
        try
        {
            return d_type switch
            {
                SecondaryPasswordDataProtectionType.AesCBC => Type_0(),
                _ => null,
            };
        }
        catch
        {
            return null;
        }

        byte[]? Type_0()
        {
            (byte[] key, byte[] iv) = AESUtils.GetParameters(secondaryPassword);
            using var aes = AESUtils.Create(key, iv, CipherMode.CBC, PaddingMode.PKCS7);
            return D___(aes, value);
        }
    }
}