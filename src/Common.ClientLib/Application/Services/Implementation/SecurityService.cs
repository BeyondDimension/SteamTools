using System.Linq;
using System.Security;
using System.Text;
using System.Threading.Tasks;
using static System.Application.Services.ISecurityService;

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
            (var content, var _) = await D2(value, secondaryPassword);
            return content;
        }

        public async ValueTask<(string? content, DResultCode resultCode)> D2(byte[]? value, string? secondaryPassword)
        {
            (var content, var resultCode) = await DB2(value, secondaryPassword);
            if (resultCode != DResultCode.Success || content == null || !content.Any()) return (null, resultCode);
            try
            {
                var content_ = Encoding.UTF8.GetString(content);
                return (content_, DResultCode.Success);
            }
            catch
            {
                return (null, DResultCode.UTF8GetStringFail);
            }
        }

        public async ValueTask<byte[]?> DB(byte[]? value, string? secondaryPassword)
        {
            (var content, var _) = await DB2(value, secondaryPassword);
            return content;
        }

        public async ValueTask<(byte[]? content, DResultCode resultCode)> DB2(byte[]? value, string? secondaryPassword)
        {
            if (value == null || value.Length == 0) return (value, DResultCode.Success);
            if (value.Length <= sizeof(int)) return (null, DResultCode.IncorrectValueFail);

            var d_type = (EncryptionMode)BitConverter.ToInt32(value, 0);
            switch (d_type)
            {
                case EncryptionMode.EmbeddedAes:
                    var value_1 = UnConcat(value);
                    var value_1_r = ea.DB(value_1);
                    return (value_1_r, value_1_r == null ?
                        DResultCode.EmbeddedAesFail : DResultCode.Success);
                case EncryptionMode.EmbeddedAesWithLocal:
                    var value_2 = UnConcat(value);
                    var value_2_local = await local.DB(value_2);
                    if (value_2_local == null) return (null, DResultCode.LocalFail);
                    var value_2_r = ea.DB(value_2_local);
                    return (value_2_r, value_2_r == null ?
                        DResultCode.EmbeddedAesFail : DResultCode.Success);
                case EncryptionMode.EmbeddedAesWithSecondaryPassword:
                    if (string.IsNullOrWhiteSpace(secondaryPassword))
                        return (null, DResultCode.SecondaryPasswordFail);
                    var value_3 = UnConcat(value);
                    var value_3_sp = sp.DB(value_3, secondaryPassword);
                    if (value_3_sp == null) return (null, DResultCode.SecondaryPasswordFail);
                    var value_3_r = ea.DB(value_3_sp);
                    return (value_3_r, value_3_r == null ?
                        DResultCode.EmbeddedAesFail : DResultCode.Success);
                case EncryptionMode.EmbeddedAesWithSecondaryPasswordWithLocal:
                    if (string.IsNullOrWhiteSpace(secondaryPassword))
                        return (null, DResultCode.SecondaryPasswordFail);
                    var value_4 = UnConcat(value);
                    var value_4_local = await local.DB(value_4);
                    if (value_4_local == null) return (null, DResultCode.LocalFail);
                    var value_4_sp = sp.DB(value_4_local, secondaryPassword);
                    if (value_4_sp == null) return (null, DResultCode.SecondaryPasswordFail);
                    var value_4_r = ea.DB(value_4_sp);
                    return (value_4_r, value_4_r == null ?
                        DResultCode.EmbeddedAesFail : DResultCode.Success);
                default:
                    return (null, DResultCode.IncorrectValueFail);
            }
        }
    }
}