using Res = BD.WTTS.MicroServices.ClientSDK.Resources;

namespace BD.WTTS.Models;

/// <summary>
/// Watt Toolkit 应用程序设置项
/// </summary>
[MPObj, MP2Obj(SerializeLayout.Explicit)]
public sealed partial class AppSettings : IMicroServiceClient.ISettings
{
    /// <summary>
    /// 版本号，已弃用
    /// </summary>
    [MPKey(0)]
    [MP2Ignore]
    [N_JsonIgnore]
    [S_JsonIgnore]
    [Obsolete("Delete", true)]
    public Guid AppVersion { get; set; }

    /// <summary>
    /// 服务端地址
    /// </summary>
    [MPKey(1), MP2Key(0)]
    [N_JsonProperty("1")]
    [S_JsonProperty("1")]
    public string? ApiBaseUrl { get; set; }

    /// <summary>
    /// AES 密钥
    /// </summary>
    [MPKey(2), MP2Key(1)]
    [N_JsonProperty("2")]
    [S_JsonProperty("2")]
    public byte[]? AesSecret { get; set; }

    Aes? aes;

    [MPIgnore]
    [MP2Ignore]
    [N_JsonIgnore]
    [S_JsonIgnore]
    public Aes Aes
    {
        get
        {
            if (aes == null)
            {
                if (AesSecret == null) throw new IsNotOfficialChannelPackageException(nameof(Aes), new ArgumentNullException(nameof(AesSecret)));
                aes = AESUtils.Create(AesSecret);
            }
            return aes;
        }
    }

    /// <summary>
    /// RSA 公钥
    /// </summary>
    [MPKey(3), MP2Key(2)]
    [N_JsonProperty("3")]
    [S_JsonProperty("3")]
    public byte[]? RSASecret { get; set; }

    RSA? rsa;

    [MPIgnore]
    [MP2Ignore]
    [N_JsonIgnore]
    [S_JsonIgnore]
    public RSA RSA
    {
        get
        {
            if (rsa == null)
            {
                if (RSASecret == null) throw new IsNotOfficialChannelPackageException(nameof(RSA), new ArgumentNullException(nameof(RSASecret)));
                rsa = RSA.Create(Serializable.DMP2<RSAParameters>(RSASecret));
            }
            return rsa;
        }
    }

    //[MPKey(4)]
    //[N_JsonProperty("4")]
    //[S_JsonProperty("4")]
    //public Guid MASLClientId { get; set; }

    //    bool? mGetIsOfficialChannelPackage;

    //    public bool GetIsOfficialChannelPackage()
    //    {
    //        bool GetIsOfficialChannelPackage_()
    //        {
    //#if SIGN_ASSEMBLY
    //            var pk = typeof(AppSettings).Assembly.GetName().GetPublicKey();
    //            if (pk == null) return false;
    //            var pkStr = ", PublicKey=" + string.Join(string.Empty, pk.Select(x => x.ToString("x2")));
    //            var r = pkStr == ThisAssembly.PublicKey;
    //            if (!r) return false;
    //#endif
    //            try
    //            {
    //                return Aes != null && RSA != null;
    //            }
    //            catch (IsNotOfficialChannelPackageException)
    //            {
    //                return false;
    //            }
    //        }
    //        if (!mGetIsOfficialChannelPackage.HasValue)
    //            mGetIsOfficialChannelPackage = GetIsOfficialChannelPackage_();
    //        return mGetIsOfficialChannelPackage.Value;
    //    }

    //    static readonly Lazy<bool> mIsOfficialChannelPackage = new(() =>
    //    {
    //        var s = Ioc.Get_Nullable<IOptions<AppSettings>>()?.Value;
    //        return s != null && s.GetIsOfficialChannelPackage();
    //    });

    //    /// <summary>
    //    /// 当前运行程序是否为官方渠道包
    //    /// </summary>
    //    public static bool IsOfficialChannelPackage => mIsOfficialChannelPackage.Value;

    public static AppSettings Instance { get; } = GetAppSettings();

#if DEBUG
    internal static bool UseLocalhostApiBaseUrl { get; set; }
#endif

    static AppSettings GetAppSettings()
    {
        var s = new AppSettings()
        {
            AesSecret = Res.aes_key,
            RSASecret = Res.
#if DEBUG
            rsa_public_key_debug,
#else
            rsa_public_key_release,
#endif
        };
        var apiBaseUrl =
#if DEBUG
            UseLocalhostApiBaseUrl ?
            Constants.Urls.BaseUrl_API_Debug :
            Constants.Urls.BaseUrl_API_Development;
#else
            Constants.Urls.BaseUrl_API_Production;
#endif
        s.ApiBaseUrl = Constants.Urls.ApiBaseUrl = apiBaseUrl;
        return s;
    }
}
