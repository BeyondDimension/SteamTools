// https://github.com/dotnetcore/FastGithub/blob/2.1.4/FastGithub.HttpServer/CertGenerator.cs

using X509Certificate2 = System.Security.Cryptography.X509Certificates.X509Certificate2;

// ReSharper disable once CheckNamespace
namespace BD.WTTS.Services.Implementation;

/// <summary>
/// 证书生成器
/// </summary>
public static class CertGenerator
{
    const string X500DistinguishedNameValue = $"C=CN, O=BeyondDimension, OU=Technical Department, CN={CertificateConstants.RootCertificateName}";

    private static readonly Oid tlsServerOid = new("1.3.6.1.5.5.7.3.1");
    private static readonly Oid tlsClientOid = new("1.3.6.1.5.5.7.3.2");

    public const int KEY_SIZE_BITS = 2048;

    /// <summary>
    /// 生成自签名证书
    /// </summary>
    ///// <param name="keySizeBits"></param>
    /// <param name="notBefore">此证书被视为有效的最早日期和时间。通常是 UtcNow，加上或减去几秒钟</param>
    /// <param name="notAfter">此证书不再被视为有效的日期和时间</param>
    /// <param name="caPfxPath"></param>
    /// <param name="password"></param>
    /// <returns></returns>
    public static X509Certificate2 GenerateBySelfPfx(
        string? x509Name,
        DateTimeOffset notBefore,
        DateTimeOffset notAfter,
        string? caPfxPath,
        string? password = default)
    {
        var r = CreateCACertificate(new X500DistinguishedName(string.IsNullOrEmpty(x509Name) ? X500DistinguishedNameValue : x509Name), notBefore, notAfter);
        if (!string.IsNullOrEmpty(caPfxPath))
        {
            byte[] exported = r.Export(X509ContentType.Pkcs12, password);
            File.WriteAllBytes(caPfxPath, exported);
        }
        return r;
    }

    /// <summary>
    /// 生成ca证书
    /// </summary>
    /// <param name="subjectName"></param>
    /// <param name="notBefore"></param>
    /// <param name="notAfter"></param>
    /// <param name="rsaKeySizeInBits"></param>
    /// <param name="pathLengthConstraint"></param>
    /// <returns></returns>
    public static X509Certificate2 CreateCACertificate(
        X500DistinguishedName subjectName,
        DateTimeOffset notBefore,
        DateTimeOffset notAfter,
        int rsaKeySizeInBits = 2048,
        int pathLengthConstraint = 1)
    {
        using var rsa = RSA.Create(rsaKeySizeInBits);
        var request = new CertificateRequest(subjectName, rsa, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);

        var basicConstraints = new X509BasicConstraintsExtension(true, pathLengthConstraint > 0, pathLengthConstraint, true);
        request.CertificateExtensions.Add(basicConstraints);

        var keyUsage = new X509KeyUsageExtension(X509KeyUsageFlags.DigitalSignature | X509KeyUsageFlags.CrlSign | X509KeyUsageFlags.KeyCertSign, true);
        request.CertificateExtensions.Add(keyUsage);

        var oids = new OidCollection { tlsServerOid, tlsClientOid };
        var enhancedKeyUsage = new X509EnhancedKeyUsageExtension(oids, true);
        request.CertificateExtensions.Add(enhancedKeyUsage);

        var dnsBuilder = new SubjectAlternativeNameBuilder();
        dnsBuilder.Add(CertificateConstants.RootCertificateName);
        request.CertificateExtensions.Add(dnsBuilder.Build());

        var subjectKeyId = new X509SubjectKeyIdentifierExtension(request.PublicKey, false);
        request.CertificateExtensions.Add(subjectKeyId);

        return request.CreateSelfSigned(notBefore, notAfter);
    }

    /// <summary>
    /// 生成服务器证书
    /// </summary>
    /// <param name="issuerCertificate"></param>
    /// <param name="subjectName"></param>
    /// <param name="extraDnsNames"></param>
    /// <param name="notBefore"></param>
    /// <param name="notAfter"></param>
    /// <param name="rsaKeySizeInBits"></param>
    /// <returns></returns>
    public static X509Certificate2 CreateEndCertificate(
        X509Certificate2 issuerCertificate,
        X500DistinguishedName subjectName,
        IEnumerable<string>? extraDnsNames = default,
        DateTimeOffset? notBefore = default,
        DateTimeOffset? notAfter = default,
        int rsaKeySizeInBits = 2048)
    {
        using var rsa = RSA.Create(rsaKeySizeInBits);
        var request = new CertificateRequest(subjectName, rsa, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);

        var basicConstraints = new X509BasicConstraintsExtension(false, false, 0, true);
        request.CertificateExtensions.Add(basicConstraints);

        var keyUsage = new X509KeyUsageExtension(X509KeyUsageFlags.DigitalSignature | X509KeyUsageFlags.KeyEncipherment, true);
        request.CertificateExtensions.Add(keyUsage);

        var oids = new OidCollection { tlsServerOid, tlsClientOid };
        var enhancedKeyUsage = new X509EnhancedKeyUsageExtension(oids, true);
        request.CertificateExtensions.Add(enhancedKeyUsage);

        var authorityKeyId = GetAuthorityKeyIdentifierExtension(issuerCertificate);
        request.CertificateExtensions.Add(authorityKeyId);

        var subjectKeyId = new X509SubjectKeyIdentifierExtension(request.PublicKey, false);
        request.CertificateExtensions.Add(subjectKeyId);

        var dnsBuilder = new SubjectAlternativeNameBuilder();
        dnsBuilder.Add(subjectName.Name[3..]);

        if (extraDnsNames != null)
        {
            foreach (var dnsName in extraDnsNames)
            {
                dnsBuilder.Add(dnsName);
            }
        }

        var dnsNames = dnsBuilder.Build();
        request.CertificateExtensions.Add(dnsNames);

        if (notBefore == null || notBefore.Value < issuerCertificate.NotBefore)
        {
            notBefore = issuerCertificate.NotBefore;
        }

        if (notAfter == null || notAfter.Value > issuerCertificate.NotAfter)
        {
            notAfter = issuerCertificate.NotAfter;
        }

        var serialNumber = BitConverter.GetBytes(Random.Shared.NextInt64());
        using var certOnly = request.Create(issuerCertificate, notBefore.Value, notAfter.Value, serialNumber);
        return certOnly.CopyWithPrivateKey(rsa);
    }

    private static void Add(this SubjectAlternativeNameBuilder builder, string name)
    {
        if (IPAddress.TryParse(name, out var address))
        {
            builder.AddIpAddress(address);
        }
        else
        {
            try
            {
                builder.AddDnsName(name);
            }
            catch
            {
                /* name = Environment.MachineName
                 * System.ArgumentException: Decoded string is not a valid IDN name. (Parameter 'unicode')
                 * at System.Globalization.IdnMapping.IcuGetAsciiCore(String unicodeString, Char* unicode, Int32 count)
                 * at System.Globalization.IdnMapping.GetAscii(String unicode, Int32 index, Int32 count)
                 * at System.Security.Cryptography.X509Certificates.SubjectAlternativeNameBuilder.AddDnsName(String dnsName)
                 * at BD.WTTS.Services.Implementation.CertGenerator.Add(SubjectAlternativeNameBuilder builder, String name)
                 * at BD.WTTS.Services.Implementation.CertGenerator.CreateEndCertificate(X509Certificate2 issuerCertificate, X500DistinguishedName subjectName, IEnumerable`1 extraDnsNames, Nullable`1 notBefore, Nullable`1 notAfter, Int32 rsaKeySizeInBits)
                 */
            }
        }
    }

    private static X509Extension GetAuthorityKeyIdentifierExtension(X509Certificate2 certificate)
    {
        var extension = new X509SubjectKeyIdentifierExtension(certificate.PublicKey, false);
#if NET7_0_OR_GREATER
        return X509AuthorityKeyIdentifierExtension.CreateFromSubjectKeyIdentifier(extension);
#else
            var subjectKeyIdentifier = extension.RawData.AsSpan(2);
            var rawData = new byte[subjectKeyIdentifier.Length + 4];
            rawData[0] = 0x30;
            rawData[1] = 0x16;
            rawData[2] = 0x80;
            rawData[3] = 0x14;
            subjectKeyIdentifier.CopyTo(rawData);

            return new X509Extension("2.5.29.35", rawData, false);
#endif
    }
}