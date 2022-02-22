using System.Security;
using System.Threading.Tasks;
using System.Application.UI.ViewModels;
using System.Application.UI.Resx;
using ArchiSteamFarm.Helpers;

namespace System.Application.Services.Implementation
{
    partial class ArchiSteamFarmServiceImpl
    {
        string EncryptionKey
        {
            set => ArchiCryptoHelper.SetEncryptionKey(value);
        }

        const string ASF_CRYPTKEY = "ASF_CRYPTKEY";
        const string ASF_CRYPTKEY_DEF_VALUE = nameof(ArchiSteamFarm);

        public async Task SetEncryptionKeyAsync()
        {
            var result = await TextBoxWindowViewModel.ShowDialogAsync(new TextBoxWindowViewModel
            {
                Title = AppResources.ASF_SetCryptKey,
                Placeholder = ASF_CRYPTKEY,
                InputType = TextBoxWindowViewModel.TextBoxInputType.Password,
            });
            var isUseDefaultCryptKey = string.IsNullOrEmpty(result) || ASF_CRYPTKEY_DEF_VALUE == result;
            if (!isUseDefaultCryptKey)
            {
                await ISecureStorage.Instance.SetAsync(ASF_CRYPTKEY, result);
            }
            else
            {
                await ISecureStorage.Instance.RemoveAsync(ASF_CRYPTKEY);
                result = ASF_CRYPTKEY_DEF_VALUE;
            }
            EncryptionKey = result!;
        }

        /// <summary>
        /// 尝试读取已保存的自定义密钥并应用
        /// </summary>
        /// <returns></returns>
        async Task ReadEncryptionKeyAsync()
        {
            if (!ArchiCryptoHelper.HasDefaultCryptKey)
            {
                // 当前运行中已设置了自定义密钥，则跳过
                return;
            }
            var result = await ISecureStorage.Instance.GetAsync(ASF_CRYPTKEY);
            if (!string.IsNullOrEmpty(result))
            {
                EncryptionKey = result;
            }
        }
    }
}


namespace System.Application.Services
{
    partial interface IArchiSteamFarmService
    {
        /// <summary>
        /// 使用弹窗密码框输入自定义密钥并设置与保存
        /// </summary>
        /// <returns></returns>
        Task SetEncryptionKeyAsync();
    }
}