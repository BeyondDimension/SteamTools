using BD.WTTS.Client.Resources;
using BD.WTTS.UI.Views.Pages;
using FluentAvalonia.UI.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WinAuth;

namespace BD.WTTS.UI.ViewModels;

public sealed partial class AuthenticatorPageViewModel : TabItemViewModel
{
    public override string Name => "AuthenticatorPage";

    public AuthenticatorPageViewModel()
    {
        ImportTabControlIsVisible = true;
        //AuthenticatorService.DeleteAllAuthenticators();
    }
    //[Reactive]
    //public ObservableCollection<string> AuthenticatorTab { get; set; }

    public bool ImportTabControlIsVisible { get; set; }

}
