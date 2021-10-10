using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Text;

// ReSharper disable once CheckNamespace
namespace System.Application.UI.ViewModels.Abstractions
{
    public partial class SettingsPageViewModel
    {
        public static SettingsPageViewModel Instance => DI.Get<SettingsPageViewModel>();

        KeyValuePair<string, string> _SelectLanguage;
        public KeyValuePair<string, string> SelectLanguage
        {
            get => _SelectLanguage;
            set => this.RaiseAndSetIfChanged(ref _SelectLanguage, value);
        }
    }
}
