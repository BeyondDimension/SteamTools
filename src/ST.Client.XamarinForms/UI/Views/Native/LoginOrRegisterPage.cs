using System;
using System.Collections.Generic;
using System.Text;
using VM = System.Application.UI.ViewModels.LoginOrRegisterWindowViewModel;

namespace System.Application.UI.Views.Native
{
    public partial class LoginOrRegisterPage : BaseContentPage<VM>
    {
        public LoginOrRegisterPage()
        {
            ViewModel = new VM();
        }
    }
}
