using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Text;

// ReSharper disable once CheckNamespace
namespace System.Application.UI.ViewModels
{
    partial class AddAuthWindowViewModel
    {
        private string? _LoginSteamLoadingText;
        public string? LoginSteamLoadingText
        {
            get => _LoginSteamLoadingText;
            set => this.RaiseAndSetIfChanged(ref _LoginSteamLoadingText, value);
        }
    }
}
