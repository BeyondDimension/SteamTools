using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Markup.Xaml;
using Avalonia.Styling;
using System.Application.UI.ViewModels;
using System.Reflection;

namespace System.Application.UI.Views.Windows
{
    public class MessageBoxWindow : FluentWindow<MessageBoxWindowViewModel>
    {
        public MessageBoxWindow() : base(false)
        {
            //if (PlatformImpl is Win32WindowImpl win)
            //{
            //    var exStyle = (WindowStyles)s_getExtendedStyle.Invoke(win, null);
            //    exStyle |= WindowStyles.WS_EX_DLGMODALFRAME;

            //    s_setExtendedStyle.Invoke(win, new object[] { exStyle, true });
            //}
            InitializeComponent();
            ExtendClientAreaChromeHints = Avalonia.Platform.ExtendClientAreaChromeHints.NoChrome;

#if DEBUG
            this.AttachDevTools();
#endif
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }

        protected override void OnOpened(EventArgs e)
        {
            base.OnOpened(e);
            Topmost = true;
            Topmost = false;
        }

        protected override void OnApplyTemplate(TemplateAppliedEventArgs e)
        {
            base.OnApplyTemplate(e);
        }
    }
}