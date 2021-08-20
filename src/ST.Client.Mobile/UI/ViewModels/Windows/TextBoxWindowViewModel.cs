using ReactiveUI;
using System.Application.Services;
using System.Application.UI.Resx;
using System.Threading.Tasks;
using System.Windows;

namespace System.Application.UI.ViewModels
{
    public class TextBoxWindowViewModel :
#if __MOBILE__
        PageViewModel
#else
        DialogWindowViewModel
#endif
        , ITextBoxWindowViewModel
    {
        private string? _Value;

        /// <summary>
        /// 文本框输入的值
        /// </summary>
        public string? Value
        {
            get => _Value;
            set => this.RaiseAndSetIfChanged(ref _Value, value);
        }

        /// <summary>
        /// 文本框的占位符
        /// </summary>
        public string? Placeholder
#if !__MOBILE__
        {
            get => _Placeholder;
            set => this.RaiseAndSetIfChanged(ref _Placeholder, value);
        }
        private string? _Placeholder;
#else
        { get; set; }
#endif

#if !__MOBILE__
        private string? _Description;
        /// <summary>
        /// 描述文本
        /// </summary>
        public string? Description
        {
            get => _Description;
            set => this.RaiseAndSetIfChanged(ref _Description, value);
        }
#endif

        /// <inheritdoc cref="TextBoxInputType"/>
        public TextBoxInputType InputType { get; set; }

        /// <summary>
        /// 输入空值时提示，<see cref="string.IsNullOrWhiteSpace(string?)"/> == <see langword="true"/> 时，在 <see cref="InputValidator"/> 中会判断输入空值时提示并取消关闭弹窗操作
        /// </summary>
        public string? ValueIsNullOrWhiteSpaceTip { get; set; }

        /// <summary>
        /// 输入空值时提示，<see cref="string.IsNullOrEmpty(string?)"/> == <see langword="true"/> 时，在 <see cref="InputValidator"/> 中会判断输入空值时提示并取消关闭弹窗操作
        /// </summary>
        public string? ValueIsNullOrEmptyTip { get; set; }

        /// <summary>
        /// 输入内容验证
        /// </summary>
        /// <returns></returns>
        public bool InputValidator()
        {
            if (ValueIsNullOrWhiteSpaceTip != null && string.IsNullOrWhiteSpace(Value))
            {
                Toast.Show(ValueIsNullOrWhiteSpaceTip);
                return false;
            }
            else if (ValueIsNullOrEmptyTip != null && string.IsNullOrEmpty(Value))
            {
                Toast.Show(ValueIsNullOrEmptyTip);
                return false;
            }
            return true;
        }

        /// <summary>
        /// 显示弹窗，点击取消按钮回返回 <see langword="null"/>
        /// </summary>
        /// <param name="vm"></param>
        /// <returns></returns>
        public static async Task<string?> ShowDialogAsync(TextBoxWindowViewModel? vm = null)
        {
            vm ??= new TextBoxWindowViewModel();
            var r = await IShowWindowService.Instance.ShowDialog(
                CustomWindow.TextBox
                , vm, string.Empty, ResizeModeCompat.NoResize);
            if (r)
                return vm.Value ?? string.Empty;
            return null;
        }

        /// <inheritdoc cref="ShowDialogAsync(TextBoxWindowViewModel?)"/>
        public static Task<string?> ShowDialogByPresetAsync(PresetType type) => type switch
        {
            PresetType.LocalAuth_PasswordRequired => ShowDialogAsync(new()
            {
                Title = AppResources.LocalAuth_PasswordRequired,
                Placeholder = AppResources.LocalAuth_PasswordRequired1,
                ValueIsNullOrEmptyTip = AppResources.LocalAuth_ProtectionAuth_PasswordErrorTip,
                InputType = TextBoxInputType.Password,
            }),
            _ => ShowDialogAsync(),
        };

        /// <summary>
        /// 文本框输入类型
        /// </summary>
        public enum TextBoxInputType
        {
            TextBox,
            Password,
            ReadOnlyText,
        }

        public enum PresetType
        {
            LocalAuth_PasswordRequired,
        }
    }
}