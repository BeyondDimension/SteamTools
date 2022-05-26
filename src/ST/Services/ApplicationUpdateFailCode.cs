using System.Application.UI.Resx;

namespace System.Application.Services
{
    public enum ApplicationUpdateFailCode : byte
    {
        /// <summary>
        /// <inheritdoc cref="SR.DownloadUpdateFail"/>
        /// </summary>
        DownloadUpdateFail,

        /// <summary>
        /// <inheritdoc cref="SR.UpdatePackVerificationFail"/>
        /// </summary>
        UpdatePackVerificationFail,

        /// <summary>
        /// <inheritdoc cref="SR.UpdatePackCacheHashInvalidDeleteFileFail_"/>
        /// </summary>
        UpdatePackCacheHashInvalidDeleteFileFail_,

        /// <summary>
        /// <inheritdoc cref="SR.UpdateEnumOutOfRange"/>
        /// </summary>
        UpdateEnumOutOfRange,

        /// <summary>
        /// <inheritdoc cref="SR.UpdateUnpackFail"/>
        /// </summary>
        UpdateUnpackFail,
    }

    public static partial class ApplicationUpdateFailCodeEnumExtensions
    {
        public static string ToString2(this ApplicationUpdateFailCode appUpdateFailCode, params string[] args) => appUpdateFailCode switch
        {
            ApplicationUpdateFailCode.DownloadUpdateFail => SR.DownloadUpdateFail,
            ApplicationUpdateFailCode.UpdatePackVerificationFail => SR.UpdatePackVerificationFail,
            ApplicationUpdateFailCode.UpdatePackCacheHashInvalidDeleteFileFail_ => SR.UpdatePackCacheHashInvalidDeleteFileFail_.Format(args),
            ApplicationUpdateFailCode.UpdateEnumOutOfRange => SR.UpdateEnumOutOfRange,
            ApplicationUpdateFailCode.UpdateUnpackFail => SR.UpdateUnpackFail,
            _ => throw new ArgumentOutOfRangeException(nameof(appUpdateFailCode), appUpdateFailCode, null)
        };
    }
}
