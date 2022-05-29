using MauiApplication = Microsoft.Maui.Controls.Application;

namespace System.Application.UI
{
    public partial class App : MauiApplication
    {
        public App()
        {
            InitializeComponent();

            MainPage = new AppShell();
        }
    }
}