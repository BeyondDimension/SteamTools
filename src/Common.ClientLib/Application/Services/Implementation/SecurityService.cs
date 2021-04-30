using System.Linq;
using System.Security;
using System.Text;
using System.Threading.Tasks;

namespace System.Application.Services.Implementation
{
    /// <inheritdoc cref="ISecurityService"/>
    internal sealed class SecurityService : ISecurityService
    {
        readonly IEmbeddedAesDataProtectionProvider ea;
        readonly ILocalDataProtectionProvider local;
        readonly ISecondaryPasswordDataProtectionProvider sp;

        public SecurityService(
            IEmbeddedAesDataProtectionProvider ea,
            ILocalDataProtectionProvider local,
            ISecondaryPasswordDataProtectionProvider sp)
        {
            this.ea = ea;
            this.local = local;
            this.sp = sp;
        }

        byte[]? Concat(byte[]? value, EncryptionMode encryptionMode)
        {
            if (value == null || !value.Any()) return null;
            var r = BitConverter.GetBytes((int)encryptionMode).Concat(value).ToArray();
            return r;
        }

        public async ValueTask<byte[]?> E(string? value, EncryptionMode encryptionMode, string? secondaryPassword)
        {
            if (string.IsNullOrEmpty(value)) return null;
            var value2 = Encoding.UTF8.GetBytes(value);
            return await EB(value2, encryptionMode, secondaryPassword);
        }

        public async ValueTask<byte[]?> EB(byte[]? value, EncryptionMode encryptionMode, string? secondaryPassword)
        {
            if (value == null) return value;
            if (value.Length == 0) return value;

            switch (encryptionMode)
            {
                case EncryptionMode.EmbeddedAes:
                    var value_1 = ea.EB(value);
                    var value_1_r = Concat(value_1, encryptionMode);
                    return value_1_r;
                case EncryptionMode.EmbeddedAesWithLocal:
                    var value_2 = ea.EB(value);
                    var value_2_local = await local.EB(value_2);
                    var value_2_r = Concat(value_2_local, encryptionMode);
                    return value_2_r;
                case EncryptionMode.EmbeddedAesWithSecondaryPassword:
                    if (string.IsNullOrWhiteSpace(secondaryPassword))
                        throw new ArgumentNullException(nameof(secondaryPassword));
                    var value_3 = ea.EB(value);
                    var value_3_sp = sp.EB(value_3, secondaryPassword);
                    var value_3_r = Concat(value_3_sp, encryptionMode);
                    return value_3_r;
                case EncryptionMode.EmbeddedAesWithSecondaryPasswordWithLocal:
                    if (string.IsNullOrWhiteSpace(secondaryPassword))
                        throw new ArgumentNullException(nameof(secondaryPassword));
                    var value_4 = ea.EB(value);
                    var value_4_sp = sp.EB(value_4, secondaryPassword);
                    var value_4_local = await local.EB(value_4_sp);
                    var value_4_r = Concat(value_4_local, encryptionMode);
                    return value_4_r;
                default:
                    throw new ArgumentOutOfRangeException(nameof(encryptionMode), encryptionMode, null);
            }
        }

        static byte[] UnConcat(byte[] value) => value.Skip(sizeof(int)).ToArray();

        public async ValueTask<string?> D(byte[]? value, string? secondaryPassword)
        {
            var value2 = await DB(value, secondaryPassword);
            if (value2 == null || !value2.Any()) return null;
            try
            {
                return Encoding.UTF8.GetString(value2);
            }
            catch
            {
                return null;
            }
        }

        public async ValueTask<byte[]?> DB(byte[]? value, string? secondaryPassword)
        {
            if (value == null) return value;
            if (value.Length == 0) return value;
            if (value.Length <= sizeof(int)) return null;

            var d_type = (EncryptionMode)BitConverter.ToInt32(value, 0);
            switch (d_type)
            {
                case EncryptionMode.EmbeddedAes:
                    var value_1 = UnConcat(value);
                    var value_1_r = ea.DB(value_1);
                    return value_1_r;
                case EncryptionMode.EmbeddedAesWithLocal:
                    var value_2 = UnConcat(value);
                    var value_2_local = await local.DB(value_2);
                    var value_2_r = ea.DB(value_2_local);
                    return value_2_r;
                case EncryptionMode.EmbeddedAesWithSecondaryPassword:
                    if (string.IsNullOrWhiteSpace(secondaryPassword)) return null;
                    var value_3 = UnConcat(value);
                    var value_3_sp = sp.DB(value_3, secondaryPassword);
                    var value_3_r = ea.DB(value_3_sp);
                    return value_3_r;
                case EncryptionMode.EmbeddedAesWithSecondaryPasswordWithLocal:
                    if (string.IsNullOrWhiteSpace(secondaryPassword)) return null;
                    var value_4 = UnConcat(value);
                    var value_4_local = await local.DB(value_4);
                    var value_4_sp = sp.DB(value_4_local, secondaryPassword);
                    var value_4_r = ea.DB(value_4_sp);
                    return value_4_r;
                default:
                    return null;
            }
        }
    }
}