using System;
using System.Collections.Generic;
using System.Text;
using ReactiveUI;

// ReSharper disable once CheckNamespace
namespace System.Application.UI.ViewModels
{
    public class MessageBoxWindowViewModel : DialogWindowViewModel
    {
        private string? _Content;
        public string? Content
        {
            get => _Content;
            set => this.RaiseAndSetIfChanged(ref _Content, value);
        }

        private bool _IsCancelcBtn;
        public bool IsCancelcBtn
        {
            get => _IsCancelcBtn;
            set => this.RaiseAndSetIfChanged(ref _IsCancelcBtn, value);
        }

        private bool _IsShowRememberChoose;
        public bool IsShowRememberChoose
        {
            get => _IsShowRememberChoose;
            set => this.RaiseAndSetIfChanged(ref _IsShowRememberChoose, value);
        }

        private bool _RememberChoose;
        public bool RememberChoose
        {
            get => _RememberChoose;
            set => this.RaiseAndSetIfChanged(ref _RememberChoose, value);
        }
    }
}
