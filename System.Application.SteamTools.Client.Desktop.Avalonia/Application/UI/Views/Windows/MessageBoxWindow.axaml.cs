using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Styling;
using System.Reflection;

namespace System.Application.UI.Windows
{
    public class MessageBoxWindow : FluentWindow
    {
        public MessageBoxWindow()
        {
            //if (PlatformImpl is Win32WindowImpl win)
            //{
            //    var exStyle = (WindowStyles)s_getExtendedStyle.Invoke(win, null);
            //    exStyle |= WindowStyles.WS_EX_DLGMODALFRAME;

            //    s_setExtendedStyle.Invoke(win, new object[] { exStyle, true });
            //}
            InitializeComponent();
#if DEBUG
            this.AttachDevTools();
#endif
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}