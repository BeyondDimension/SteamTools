using System.Properties;

namespace System.Application.UI.ViewModels
{
    partial class TaskBarWindowViewModel
    {
        public static string TitleString => ThisAssembly.AssemblyTrademark;

        public const string CommandExit = "Exit";
    }
}