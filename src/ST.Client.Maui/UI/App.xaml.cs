using MauiApplication = Microsoft.Maui.Controls.Application;
using UI_Styles_Colors_xaml = __XamlGeneratedCode__.__TypeD77312341DE78E28;
using UI_Styles_Styles_xaml = __XamlGeneratedCode__.__TypeE0CAB8CC0B793AB6;

namespace System.Application.UI;

public partial class App : MauiApplication
{
    public App()
    {
        InitializeComponent();

        Resources.MergedDictionaries.Add(new UI_Styles_Colors_xaml());
        Resources.MergedDictionaries.Add(new UI_Styles_Styles_xaml());

        MainPage = new AppShell();
    }
}