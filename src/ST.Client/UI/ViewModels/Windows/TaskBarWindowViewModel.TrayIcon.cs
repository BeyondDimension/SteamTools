using System.Properties;

// ReSharper disable once CheckNamespace
namespace System.Application.UI.ViewModels
{
    partial class TaskBarWindowViewModel
    {
        public static string TitleString => ThisAssembly.DisplayTrademark;


        public const string CommandExit = "Exit";
    }
}