using System.Security.Cryptography;
using System.Threading.Tasks;
using Xamarin.Essentials;
using static System.Application.Services.ILocalDataProtectionProvider;
using IProtectedData = System.Application.Services.ILocalDataProtectionProvider.IProtectedData;

namespace System.Application.Services.Implementation
{
    /// <inheritdoc cref="ILocalDataProtectionProvider"/>
    public class LocalDataProtectionProvider : LocalDataProtectionProviderBase
    {
        public LocalDataProtectionProvider(IProtectedData protectedData, IDataProtectionProvider dataProtectionProvider) : base(protectedData, dataProtectionProvider)
        {
            MachineSecretKey = GetMachineSecretKey();
        }

        const string KEY_MACHINE_SECRET = "KEY_MACHINE_SECRET_2105";

        static Guid GetMachineSecretKeyGuid()
        {
            Func<Task<string>> getAsync = () => SecureStorage.GetAsync(KEY_MACHINE_SECRET);
            var guidStr = getAsync.RunSync();
            if (Guid.TryParse(guidStr, out var guid)) return guid;
            guid = Guid.NewGuid();
            guidStr = guid.ToString();
            Func<Task> setAsync = () => SecureStorage.SetAsync(KEY_MACHINE_SECRET, guidStr);
            setAsync.RunSync();
            return guid;
        }

        static (byte[] key, byte[] iv) GetMachineSecretKey()
        {
            var guid = GetMachineSecretKeyGuid();
            var r = AESUtils.GetParameters(guid.ToByteArray());
            return r;
        }

        protected override (byte[] key, byte[] iv) MachineSecretKey { get; }
    }
}