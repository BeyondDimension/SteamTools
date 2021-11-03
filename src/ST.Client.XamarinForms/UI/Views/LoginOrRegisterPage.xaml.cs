using ReactiveUI;
using System;
using System.Application.UI.ViewModels;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using VM = System.Application.UI.ViewModels.LoginOrRegisterWindowViewModel;

namespace System.Application.UI.Views
{
    public partial class LoginOrRegisterPage : BaseContentPage<VM>
    {
        public LoginOrRegisterPage()
        {
            InitializeComponent();
            ViewModel = new VM
            {
                TbPhoneNumberReturnCommand = ReactiveCommand.Create<Entry>(textBox =>
                {
                    if (BindingContext is VM vm) vm.SendSms.Invoke();
                    textBox?.Focus();
                }),
                Close = AppShell.Pop,
            };
        }
    }
}