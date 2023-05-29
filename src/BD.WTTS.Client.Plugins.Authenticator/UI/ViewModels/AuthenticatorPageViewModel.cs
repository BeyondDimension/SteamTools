using BD.WTTS.UI.Views.Pages;
using FluentAvalonia.UI.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BD.WTTS.UI.ViewModels;

public sealed partial class AuthenticatorPageViewModel
{
    [Reactive]
    public ObservableCollection<string> AuthenticatorTab { get; set; }

    public bool MainTabControlIsVisible { get; set; }

    public AuthenticatorPageViewModel()
    {
        MainTabControlIsVisible = true;
        AuthenticatorTab = new()
        {
            "拥有令牌",
            "新增令牌",
            "导出令牌"
        };
    }

}
