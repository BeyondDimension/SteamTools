// https://github.com/dotnetcore/FastGithub/blob/2.1.4/FastGithub.HttpServer/CertGenerator.cs

using Org.BouncyCastle.Asn1.Pkcs;
using Org.BouncyCastle.Asn1.X509;
using Org.BouncyCastle.Asn1.X9;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Generators;
using Org.BouncyCastle.Crypto.Operators;
using Org.BouncyCastle.Crypto.Parameters;
using Org.BouncyCastle.Crypto.Prng;
using Org.BouncyCastle.Math;
using Org.BouncyCastle.OpenSsl;
using Org.BouncyCastle.Pkcs;
using Org.BouncyCastle.Security;
using Org.BouncyCastle.X509;
using Org.BouncyCastle.X509.Extension;
using System.ComponentModel;
using System.Net;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using X509Certificate = Org.BouncyCastle.X509.X509Certificate;
using X509Certificate2 = System.Security.Cryptography.X509Certificates.X509Certificate2;

namespace System.Application.Services.Implementation.HttpServer.Certificates;

/// <summary>
/// 证书生成器
/// </summary>
static class CertGenerator
{
    static readonly SecureRandom secureRandom = new();

    /// <summary>
    /// 生成自签名证书
    /// </summary>
    /// <param name="domains"></param>
    /// <param name="keySizeBits"></param>
    /// <param name="validFrom"></param>
    /// <param name="validTo"></param>
    /// <param name="caPublicCerPath"></param>
    /// <param name="caPrivateKeyPath"></param>
    public static void GenerateBySelf(IEnumerable<string> domains, int keySizeBits, DateTime validFrom, DateTime validTo, string caPublicCerPath, string? caPrivateKeyPath)
    {
        var keys = GenerateRsaKeyPair(keySizeBits);
        var cert = GenerateCertificate(domains, keys.Public, validFrom, validTo, domains.First(), null, keys.Private, 1);

        if (!string.IsNullOrWhiteSpace(caPrivateKeyPath))
        {
            using var priWriter = new StreamWriter(caPrivateKeyPath);
            var priPemWriter = new PemWriter(priWriter);
            priPemWriter.WriteObject(keys.Private);
            priPemWriter.Writer.Flush();
        }

        using var pubWriter = new StreamWriter(caPublicCerPath);
        var pubPemWriter = new PemWriter(pubWriter);
        pubPemWriter.WriteObject(cert);
        pubPemWriter.Writer.Flush();
    }

    /// <summary>
    /// 生成自签名证书
    /// </summary>
    /// <param name="domains"></param>
    /// <param name="keySizeBits"></param>
    /// <param name="validFrom"></param>
    /// <param name="validTo"></param>
    /// <param name="caPfxPath"></param>
    /// <param name="password"></param>
    /// <returns></returns>
    public static X509Certificate2 GenerateBySelfPfx(IEnumerable<string> domains, int keySizeBits, DateTime validFrom, DateTime validTo, string? caPfxPath, string? password = default)
    {
        var keys = GenerateRsaKeyPair(keySizeBits);
        var cert = GenerateCertificate(domains, keys.Public, validFrom, validTo, domains.First(), null, keys.Private, 1);

        var x509Certificate = WithPrivateKey(cert, keys.Private);

        if (!string.IsNullOrEmpty(caPfxPath))
        {
            byte[] exported = x509Certificate.Export(X509ContentType.Pkcs12, password);
            File.WriteAllBytes(caPfxPath, exported);
        }

        return x509Certificate;
    }

    /// <summary>
    /// 生成 CA 签名证书
    /// </summary>
    /// <param name="domains"></param>
    /// <param name="keySizeBits"></param>
    /// <param name="validFrom"></param>
    /// <param name="validTo"></param>
    /// <param name="caPublicCerPath"></param>
    /// <param name="caPrivateKeyPath"></param>
    /// <param name="password"></param>
    /// <returns></returns>
    /// <exception cref="FileNotFoundException"></exception>
    public static X509Certificate2 GenerateByCa(IEnumerable<string> domains, int keySizeBits, DateTime validFrom, DateTime validTo, string caPublicCerPath, string caPrivateKeyPath, string? password = default)
    {
        if (File.Exists(caPublicCerPath) == false)
        {
            throw new FileNotFoundException(caPublicCerPath);
        }

        if (File.Exists(caPrivateKeyPath) == false)
        {
            throw new FileNotFoundException(caPublicCerPath);
        }

        using var pubReader = new StreamReader(caPublicCerPath, Encoding.ASCII);
        var caCert = (X509Certificate)new PemReader(pubReader).ReadObject();

        using var priReader = new StreamReader(caPrivateKeyPath, Encoding.ASCII);
        var reader = new PemReader(priReader);
        var caPrivateKey = ((AsymmetricCipherKeyPair)reader.ReadObject()).Private;

        var caSubjectName = GetSubjectName(caCert);
        var keys = GenerateRsaKeyPair(keySizeBits);
        var cert = GenerateCertificate(domains, keys.Public, validFrom, validTo, caSubjectName, caCert.GetPublicKey(), caPrivateKey, null);

        return GeneratePfx(cert, keys.Private, password);
    }

    /// <summary>
    /// 生成 CA 签名证书
    /// </summary>
    /// <param name="domains"></param>
    /// <param name="keySizeBits"></param>
    /// <param name="validFrom"></param>
    /// <param name="validTo"></param>
    /// <param name="caPfxPath"></param>
    /// <param name="password"></param>
    /// <returns></returns>
    /// <exception cref="FileNotFoundException"></exception>
    public static X509Certificate2 GenerateByCaPfx(IEnumerable<string> domains, int keySizeBits, DateTime validFrom, DateTime validTo, string caPfxPath, string? password = default)
    {
        if (File.Exists(caPfxPath) == false)
        {
            throw new FileNotFoundException(caPfxPath);
        }

        var ca = new X509Certificate2(caPfxPath, password, X509KeyStorageFlags.Exportable);

        var publicKey = DotNetUtilities.GetRsaPublicKey(ca.GetRSAPublicKey());
        AsymmetricKeyParameter privateKey;
        try
        {
            privateKey = DotNetUtilities.GetRsaKeyPair(ca.GetRSAPrivateKey()).Private;
        }
        catch (CryptographicException)
        {
            if (OperatingSystem.IsWindows())
            {
                // https://github.com/dotnet/runtime/issues/26031
                // https://github.com/dotnet/runtime/issues/36899
                var exported = ca.GetRSAPrivateKey()!.ExportEncryptedPkcs8PrivateKey(password, new PbeParameters(PbeEncryptionAlgorithm.TripleDes3KeyPkcs12, HashAlgorithmName.SHA1, 1));
                RSA temp = RSA.Create();
                temp.ImportEncryptedPkcs8PrivateKey(password, exported, out _);
                var loadedPrivate = temp.ExportParameters(true);
                privateKey = DotNetUtilities.GetRsaKeyPair(loadedPrivate).Private;
            }
            else
            {
                throw;
            }
        }

        var caSubjectName = GetSubjectName(ca);
        var keys = GenerateRsaKeyPair(keySizeBits);
        var cert = GenerateCertificate(domains, keys.Public, validFrom, validTo, caSubjectName, publicKey, privateKey, null);

        return GeneratePfx(cert, keys.Private, password);
    }

    /// <summary>
    /// 生成私钥
    /// </summary>
    /// <param name="length"></param>
    /// <returns></returns>
    static AsymmetricCipherKeyPair GenerateRsaKeyPair(int length)
    {
        var keygenParam = new KeyGenerationParameters(secureRandom, length);
        var keyGenerator = new RsaKeyPairGenerator();
        keyGenerator.Init(keygenParam);
        return keyGenerator.GenerateKeyPair();
    }

    /// <summary>
    /// 生成证书
    /// </summary>
    /// <param name="domains"></param>
    /// <param name="subjectPublic"></param>
    /// <param name="validFrom"></param>
    /// <param name="validTo"></param>
    /// <param name="issuerName"></param>
    /// <param name="issuerPublic"></param>
    /// <param name="issuerPrivate"></param>
    /// <param name="caPathLengthConstraint"></param>
    /// <returns></returns>
    static X509Certificate GenerateCertificate(IEnumerable<string> domains, AsymmetricKeyParameter subjectPublic, DateTime validFrom, DateTime validTo, string issuerName, AsymmetricKeyParameter? issuerPublic, AsymmetricKeyParameter issuerPrivate, int? caPathLengthConstraint)
    {
        var signatureFactory = issuerPrivate is ECPrivateKeyParameters
            ? new Asn1SignatureFactory(X9ObjectIdentifiers.ECDsaWithSha256.ToString(), issuerPrivate)
            : new Asn1SignatureFactory(PkcsObjectIdentifiers.Sha256WithRsaEncryption.ToString(), issuerPrivate);

        var certGenerator = new X509V3CertificateGenerator();
        certGenerator.SetIssuerDN(new X509Name("CN=" + issuerName));
        certGenerator.SetSubjectDN(new X509Name("CN=" + domains.First()));
        certGenerator.SetSerialNumber(BigInteger.ProbablePrime(120, new Random()));
        certGenerator.SetNotBefore(validFrom);
        certGenerator.SetNotAfter(validTo);
        certGenerator.SetPublicKey(subjectPublic);

        if (issuerPublic != null)
        {
            var akis = new AuthorityKeyIdentifierStructure(issuerPublic);
            certGenerator.AddExtension(X509Extensions.AuthorityKeyIdentifier, false, akis);
        }

        if (caPathLengthConstraint != null && caPathLengthConstraint >= 0)
        {
            var basicConstraints = new BasicConstraints(caPathLengthConstraint.Value);
            certGenerator.AddExtension(X509Extensions.BasicConstraints, true, basicConstraints);
            certGenerator.AddExtension(X509Extensions.KeyUsage, false, new KeyUsage(KeyUsage.DigitalSignature | KeyUsage.CrlSign | KeyUsage.KeyCertSign));
        }
        else
        {
            var basicConstraints = new BasicConstraints(cA: false);
            certGenerator.AddExtension(X509Extensions.BasicConstraints, true, basicConstraints);
            certGenerator.AddExtension(X509Extensions.KeyUsage, false, new KeyUsage(KeyUsage.DigitalSignature | KeyUsage.KeyEncipherment));
        }
        certGenerator.AddExtension(X509Extensions.ExtendedKeyUsage, true, new ExtendedKeyUsage(KeyPurposeID.IdKPServerAuth));

        var names = domains.Select(domain =>
        {
            var nameType = GeneralName.DnsName;
            if (IPAddress.TryParse(domain, out _))
            {
                nameType = GeneralName.IPAddress;
            }
            return new GeneralName(nameType, domain);
        }).ToArray();

        var subjectAltName = new GeneralNames(names);
        certGenerator.AddExtension(X509Extensions.SubjectAlternativeName, false, subjectAltName);
        return certGenerator.Generate(signatureFactory);
    }

    /// <summary>
    /// 生成 PFX
    /// </summary>
    /// <param name="cert"></param>
    /// <param name="privateKey"></param>
    /// <param name="password"></param>
    /// <returns></returns>
    static X509Certificate2 GeneratePfx(X509Certificate cert, AsymmetricKeyParameter privateKey, string? password)
    {
        var subject = GetSubjectName(cert);
        var pkcs12Store = new Pkcs12Store();
        var certEntry = new X509CertificateEntry(cert);
        pkcs12Store.SetCertificateEntry(subject, certEntry);
        pkcs12Store.SetKeyEntry(subject, new AsymmetricKeyEntry(privateKey), new[] { certEntry, });

        using var pfxStream = new MemoryStream();
        pkcs12Store.Save(pfxStream, password?.ToCharArray(), secureRandom);
        return new X509Certificate2(pfxStream.ToArray());
    }

    static X509Certificate2 WithPrivateKey(X509Certificate certificate, AsymmetricKeyParameter privateKey)
    {
        const string password = "password";
        Pkcs12Store store;

        if (OperatingSystem.IsAndroid())
        {
            var builder = new Pkcs12StoreBuilder();
            builder.SetUseDerEncoding(true);
            // https://github.com/dotnet/runtime/issues/71603
            builder.SetCertAlgorithm(PkcsObjectIdentifiers.PbeWithShaAnd3KeyTripleDesCbc);
            store = builder.Build();
        }
        else if (OperatingSystem2.IsRunningOnMono())
        {
            var builder = new Pkcs12StoreBuilder();
            builder.SetUseDerEncoding(true);
            store = builder.Build();
        }
        else
        {
            store = new Pkcs12Store();
        }

        var entry = new X509CertificateEntry(certificate);
        store.SetCertificateEntry(certificate.SubjectDN.ToString(), entry);

        store.SetKeyEntry(certificate.SubjectDN.ToString(), new AsymmetricKeyEntry(privateKey), new[] { entry });
        using var ms = new MemoryStream();
        store.Save(ms, password.ToCharArray(), new SecureRandom(new CryptoApiRandomGenerator()));

        return new X509Certificate2(ms.ToArray(), password, X509KeyStorageFlags.Exportable);
    }

    /// <summary>
    /// 获取 Subject
    /// </summary>
    /// <param name="cert"></param>
    /// <returns></returns>
    static string GetSubjectName(X509Certificate cert)
    {
        var subject = cert.SubjectDN.ToString();
        if (subject.StartsWith("CN=", StringComparison.OrdinalIgnoreCase))
        {
            subject = subject[3..];
        }
        return subject;
    }

    /// <summary>
    /// 获取 Subject
    /// </summary>
    /// <param name="cert"></param>
    /// <returns></returns>
    static string GetSubjectName(X509Certificate2 cert)
    {
        var subject = cert.SubjectName.Name;
        if (subject.StartsWith("CN=", StringComparison.OrdinalIgnoreCase))
        {
            subject = subject[3..];
        }
        return subject;
    }
}
