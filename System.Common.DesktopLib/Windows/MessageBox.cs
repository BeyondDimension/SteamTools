extern alias MessageBox_Avalonia;

using Avalonia.Controls;
using MessageBox_Avalonia::MessageBox.Avalonia;
using MessageBox_Avalonia::MessageBox.Avalonia.BaseWindows.Base;
using MessageBox_Avalonia::MessageBox.Avalonia.DTO;
using MessageBox_Avalonia::MessageBox.Avalonia.Enums;
using System.Reflection;
using System.Threading.Tasks;

namespace System.Windows
{
    /// <summary>
    /// 显示消息框。
    /// </summary>
    public static class MessageBox
    {
        static ButtonEnum GetButtonEnum(this MessageBoxButton button) => button switch
        {
            MessageBoxButton.OK => ButtonEnum.Ok,
            MessageBoxButton.OKCancel => ButtonEnum.OkCancel,
            MessageBoxButton.YesNo => ButtonEnum.YesNo,
            MessageBoxButton.YesNoCancel => ButtonEnum.YesNoCancel,
#pragma warning disable CS0618 // 类型或成员已过时
            MessageBoxButton.OkAbort => ButtonEnum.OkAbort,
            MessageBoxButton.YesNoAbort => ButtonEnum.YesNoAbort,
#pragma warning restore CS0618 // 类型或成员已过时
            _ => throw new ArgumentOutOfRangeException(nameof(button), $"value: {button}"),
        };

        static Icon GetIcon(this MessageBoxImage icon) => icon switch
        {
            MessageBoxImage.Asterisk => Icon.Info,
            MessageBoxImage.Error => Icon.Error,
            MessageBoxImage.Exclamation => Icon.Warning,
            MessageBoxImage.None => Icon.None,
#pragma warning disable CS0618 // 类型或成员已过时
            MessageBoxImage.Question => Icon.Info,
            MessageBoxImage.Battery => Icon.Battery,
            MessageBoxImage.Database => Icon.Database,
            MessageBoxImage.Folder => Icon.Folder,
            MessageBoxImage.Forbidden => Icon.Forbidden,
            MessageBoxImage.Plus => Icon.Plus,
            MessageBoxImage.Setting => Icon.Setting,
            MessageBoxImage.SpeakerLess => Icon.SpeakerLess,
            MessageBoxImage.SpeakerMore => Icon.SpeakerMore,
            MessageBoxImage.Stop2 => Icon.Stop,
            MessageBoxImage.Stopwatch => Icon.Stopwatch,
            MessageBoxImage.Wifi => Icon.Wifi,
#pragma warning restore CS0618 // 类型或成员已过时
            _ => throw new ArgumentOutOfRangeException(nameof(icon), $"value: {icon}"),
        };

        static MessageBoxResult GetResult(this ButtonResult result) => result switch
        {
            ButtonResult.Ok => MessageBoxResult.OK,
            ButtonResult.Yes => MessageBoxResult.Yes,
            ButtonResult.No => MessageBoxResult.No,
#pragma warning disable CS0618 // 类型或成员已过时
            ButtonResult.Abort => MessageBoxResult.Abort,
#pragma warning restore CS0618 // 类型或成员已过时
            ButtonResult.Cancel => MessageBoxResult.Cancel,
            ButtonResult.None => MessageBoxResult.None,
            _ => throw new ArgumentOutOfRangeException(nameof(result), $"value: {result}"),
        };

        /// <summary>
        /// 显示一个消息框，该消息框包含消息、标题栏标题、按钮和图标，并且返回结果。
        /// </summary>
        /// <param name="messageBoxText">一个 <see cref="string"/>，用于指定要显示的文本。</param>
        /// <param name="caption">一个 <see cref="string"/>，用于指定要显示的标题栏标题。</param>
        /// <param name="button">一个 <see cref="MessageBoxButton"/> 值，用于指定要显示哪个按钮或哪些按钮。</param>
        /// <param name="icon">一个 <see cref="MessageBoxImage"/> 值，用于指定要显示的图标。</param>
        /// <returns>一个 <see cref="MessageBoxResult"/> 值，用于指定用户单击了哪个消息框按钮。</returns>
        public static async Task<MessageBoxResult> ShowAsync(
            string messageBoxText, string caption, MessageBoxButton button, MessageBoxImage icon)
        {
            var parms = new MessageBoxStandardParams
            {
                ButtonDefinitions = button.GetButtonEnum(),
                ContentTitle = caption,
                ContentMessage = messageBoxText,
                Icon = icon.GetIcon(),
                Style = Style.None,
                WindowStartupLocation = WindowStartupLocation.CenterScreen,
            };
            var msBoxStandardWindow = MessageBoxManager.GetMessageBoxStandardWindow(parms);
            var window = GetWindow(msBoxStandardWindow);
            if (window != null)
            {
                window.Topmost = true;
                window.SetResizeMode(ResizeMode.NoResize);
                window.SetDefaultFontFamily();
            }
            var result = await msBoxStandardWindow.Show();
            return GetResult(result);
        }

        static Window? GetWindow(IMsBoxWindow<ButtonResult> window)
        {
            var _window = window.GetType().GetField("_window", BindingFlags.NonPublic | BindingFlags.Instance);
            if (_window != null && _window.FieldType.IsSubclassOf(typeof(Window)))
            {
                if (_window.GetValue(window) is Window w)
                {
                    return w;
                }
            }
            return null;
        }
    }
}