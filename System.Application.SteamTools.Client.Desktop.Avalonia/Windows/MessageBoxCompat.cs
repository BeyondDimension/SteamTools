extern alias MessageBox_Avalonia;

using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using MessageBox_Avalonia::MessageBox.Avalonia;
using MessageBox_Avalonia::MessageBox.Avalonia.BaseWindows.Base;
using MessageBox_Avalonia::MessageBox.Avalonia.DTO;
using MessageBox_Avalonia::MessageBox.Avalonia.Enums;
using System.Application.Services;
using System.Reflection;
using System.Threading.Tasks;
using AvaloniaApplication = Avalonia.Application;

namespace System.Windows
{
    /// <summary>
    /// 显示消息框。
    /// </summary>
    public static class MessageBoxCompat
    {
        static ButtonEnum GetButtonEnum(this MessageBoxButtonCompat button) => button switch
        {
            MessageBoxButtonCompat.OK => ButtonEnum.Ok,
            MessageBoxButtonCompat.OKCancel => ButtonEnum.OkCancel,
            MessageBoxButtonCompat.YesNo => ButtonEnum.YesNo,
            MessageBoxButtonCompat.YesNoCancel => ButtonEnum.YesNoCancel,
#pragma warning disable CS0618 // 类型或成员已过时
            MessageBoxButtonCompat.OkAbort => ButtonEnum.OkAbort,
            MessageBoxButtonCompat.YesNoAbort => ButtonEnum.YesNoAbort,
#pragma warning restore CS0618 // 类型或成员已过时
            _ => throw new ArgumentOutOfRangeException(nameof(button), $"value: {button}"),
        };

        static Icon GetIcon(this MessageBoxImageCompat icon) => icon switch
        {
            MessageBoxImageCompat.Asterisk => Icon.Info,
            MessageBoxImageCompat.Error => Icon.Error,
            MessageBoxImageCompat.Exclamation => Icon.Warning,
            MessageBoxImageCompat.None => Icon.None,
#pragma warning disable CS0618 // 类型或成员已过时
            MessageBoxImageCompat.Question => Icon.Info,
            MessageBoxImageCompat.Battery => Icon.Battery,
            MessageBoxImageCompat.Database => Icon.Database,
            MessageBoxImageCompat.Folder => Icon.Folder,
            MessageBoxImageCompat.Forbidden => Icon.Forbidden,
            MessageBoxImageCompat.Plus => Icon.Plus,
            MessageBoxImageCompat.Setting => Icon.Setting,
            MessageBoxImageCompat.SpeakerLess => Icon.SpeakerLess,
            MessageBoxImageCompat.SpeakerMore => Icon.SpeakerMore,
            MessageBoxImageCompat.Stop2 => Icon.Stop,
            MessageBoxImageCompat.Stopwatch => Icon.Stopwatch,
            MessageBoxImageCompat.Wifi => Icon.Wifi,
#pragma warning restore CS0618 // 类型或成员已过时
            _ => throw new ArgumentOutOfRangeException(nameof(icon), $"value: {icon}"),
        };

        static MessageBoxResultCompat GetResult(this ButtonResult result) => result switch
        {
            ButtonResult.Ok => MessageBoxResultCompat.OK,
            ButtonResult.Yes => MessageBoxResultCompat.Yes,
            ButtonResult.No => MessageBoxResultCompat.No,
#pragma warning disable CS0618 // 类型或成员已过时
            ButtonResult.Abort => MessageBoxResultCompat.Abort,
#pragma warning restore CS0618 // 类型或成员已过时
            ButtonResult.Cancel => MessageBoxResultCompat.Cancel,
            ButtonResult.None => MessageBoxResultCompat.None,
            _ => throw new ArgumentOutOfRangeException(nameof(result), $"value: {result}"),
        };

        /// <inheritdoc cref="IMessageBoxCompatService.ShowAsync(string, string, MessageBoxButtonCompat, MessageBoxImageCompat)"/>
        public static async Task<MessageBoxResultCompat> ShowAsync(
            string messageBoxText, string caption, MessageBoxButtonCompat button, MessageBoxImageCompat icon)
        {
            var f = DI.Get_Nullable<IMessageBoxCompatService>();

            if (f != null)
            {
                return await f.ShowAsync(messageBoxText, caption, button, icon);
            }

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
                window.SetResizeMode(ResizeModeCompat.NoResize);
                window.SetDefaultFontFamily();
            }

            ButtonResult? result = null;

            if (AvaloniaApplication.Current.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                var ownerWindow = desktop.MainWindow;
                if (ownerWindow != null && ownerWindow.IsInitialized && ownerWindow.IsVisible)
                {
                    result = await msBoxStandardWindow.ShowDialog(ownerWindow);
                }
            }

            if (!result.HasValue)
            {
                result = await msBoxStandardWindow.Show();
            }

            return GetResult(result.Value);
        }

        /// <inheritdoc cref="IMessageBoxCompatService.ShowAsync(string, string, MessageBoxButtonCompat, MessageBoxImageCompat)"/>
        public static async void Show(string messageBoxText, string caption, MessageBoxButtonCompat button, MessageBoxImageCompat icon)
        {
            await ShowAsync(messageBoxText, caption, button, icon);
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