namespace BD.WTTS.Client.Tools.Publish.Commands;

interface IGenerateCaCertCommand : ICommand
{
    const string commandName = "gcert";
    const string X500DistinguishedNameValue = $"C=CN, S=Hunan, L=Changsha, O=\u5F90\u5DDE\u7E41\u661F\u7F51\u7EDC\u79D1\u6280\u6709\u9650\u516C\u53F8, OU=\u6280\u672F\u90E8, CN=\u5F90\u5DDE\u7E41\u661F\u7F51\u7EDC\u79D1\u6280\u6709\u9650\u516C\u53F8";

    public const int CertificateValidDays = 365;

    public const int KEY_SIZE_BITS = 4096;

    static Command ICommand.GetCommand()
    {
        var type = new Option<int>("--type",
            "Generate certificate type(1 code, 2 store-code)");
        var path = new Option<string>("--path",
            "Directory where certificate files need to be saved");
        var cn = new Option<string>("--cn", "X500DistinguishedNameValue");
        var password = new Option<string>("--pwd");
        var command = new Command(commandName, "Generate certificate")
        {
            type, path, cn, password,
        };
        command.SetHandler(Handler, type, path, cn, password);
        return command;
    }

    // New-SelfSignedCertificate -Type Custom -Subject "CN=Contoso Software, O=Contoso Corporation, C=US" -KeyUsage DigitalSignature -FriendlyName "Your friendly name goes here" -CertStoreLocation "Cert:\CurrentUser\My" -TextExtension @("2.5.29.37={text}1.3.6.1.5.5.7.3.3", "2.5.29.19={text}")

    /// <summary>
    /// 表示证书对代码签名有效。始终指定此值以限制证书的预期用途。
    /// https://oidref.com/1.3.6.1.5.5.7.3.3
    /// </summary>
    const string codeSigningOid = "1.3.6.1.5.5.7.3.3";

    /// <summary>
    /// 表示证书遵循生存期签名。通常，如果签名带有时间戳，只要证书在时间戳时有效，即使证书过期，签名也仍然有效。此 EKU 强制签名过期，无论签名是否带有时间戳。
    /// https://oidref.com/1.3.6.1.4.1.311.10.3.13
    /// </summary>
    const string _13Oid = "1.3.6.1.4.1.311.10.3.13";

    static (byte[] pfx, byte[] cer, string? csr) GenerateCodeSigningCert(string x500DistinguishedNameValue, string password, bool isCSR = false)
    {
        string? csr = default;
        if (!isCSR && string.IsNullOrWhiteSpace(password))
            throw new ArgumentOutOfRangeException(nameof(password), password, null);

        // https://learn.microsoft.com/zh-cn/windows/win32/appxpkg/how-to-create-a-package-signing-certificate

        using var rsa = RSA.Create(KEY_SIZE_BITS);
        X500DistinguishedName subjectName = new(x500DistinguishedNameValue);
        CertificateRequest request = new(subjectName, rsa,
            HashAlgorithmName.SHA384, RSASignaturePadding.Pkcs1);

        // 基本约束：此扩展指示证书是否为证书颁发机构 (CA) 。 对于自签名证书，
        // 此参数应包含扩展字符串 “2.5.29.19={text}”，指示证书是最终实体 (不是 CA) 。
        X509BasicConstraintsExtension basicConstraints = new(false, false, 0, true);
        request.CertificateExtensions.Add(basicConstraints);

        X509KeyUsageExtension keyUsage = new(X509KeyUsageFlags.DigitalSignature | X509KeyUsageFlags.CrlSign | X509KeyUsageFlags.KeyCertSign, true);
        request.CertificateExtensions.Add(keyUsage);

        var oids = new OidCollection { new(codeSigningOid), new(_13Oid), };
        var enhancedKeyUsage = new X509EnhancedKeyUsageExtension(oids, true);
        request.CertificateExtensions.Add(enhancedKeyUsage);

        X509SubjectKeyIdentifierExtension subjectKeyId;

        subjectKeyId = new(request.PublicKey, false);
        request.CertificateExtensions.Add(subjectKeyId);

        var notBefore = DateTimeOffset.UtcNow;
        var notAfter = notBefore.AddDays(CertificateValidDays);
        using var cert = request.CreateSelfSigned(notBefore, notAfter);
        var pfx = cert.Export(X509ContentType.Pfx, password); // 私钥
        var cer = cert.Export(X509ContentType.Cert); // 公钥

        if (isCSR)
        {
            csr = request.CreateSigningRequestPem();
        }

        return (pfx, cer, csr);
    }

    internal static void Handler(int type, string path, string cn, string password)
    {
        if (string.IsNullOrWhiteSpace(path))
            throw new ArgumentOutOfRangeException(nameof(path), path, null);

        (var pfx, var cer, var csr) = type switch
        {
            // 生成 BeyondDimension 代码签名证书
            1 => GenerateCodeSigningCert(X500DistinguishedNameValue, password, true),
            // 生成 上传应用商店的 代码签名证书
            2 => GenerateCodeSigningCert($"CN={cn}", password),
            _ => throw new ArgumentOutOfRangeException(nameof(type), type, null),
        };
        File.WriteAllBytes(path, pfx);
        File.WriteAllBytes(path.TrimEnd(".pfx", StringComparison.OrdinalIgnoreCase) + ".cer", cer);
        if (csr != default)
        {
            File.WriteAllText(path.TrimEnd(".pfx", StringComparison.OrdinalIgnoreCase) + ".txt", csr);
        }
    }
}
