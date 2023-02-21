// ReSharper disable once CheckNamespace
namespace BD.WTTS.Services.Implementation;

/// <inheritdoc cref="EmbeddedAesDataProtectionProviderBase"/>
public class EmbeddedAesDataProtectionProvider : EmbeddedAesDataProtectionProviderBase
{
    const string TAG = nameof(EmbeddedAesDataProtectionProvider);

    protected readonly AppSettings settings;

    public EmbeddedAesDataProtectionProvider(IOptions<AppSettings> options)
    {
        settings = options.Value;
    }

    Aes[]? aes;
    bool isCallGetAes;

    public override Aes[]? Aes
    {
        get
        {
            if (aes != null)
            {
                return aes;
            }
            else if (isCallGetAes)
            {
                return null;
            }
            try
            {
                aes = new[] { settings.Aes };
                return aes;
            }
            catch (IsNotOfficialChannelPackageException e)
            {
                isCallGetAes = true;
                Log.Error(TAG, e, nameof(ApiRspCode.IsNotOfficialChannelPackage));
                return null;
            }
        }
    }
}