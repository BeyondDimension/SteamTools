using System;
using System.Application.Services;
using System.Application.UI.ViewModels;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xamarin.Forms;

namespace System.Application.UI.Views
{
    public partial class LocalAuthPage : BaseContentPage<LocalAuthPageViewModel>
    {
        public LocalAuthPage()
        {
            ViewModel = IViewModelManager.Instance.GetMainPageViewModel<LocalAuthPageViewModel>();

            Actions = InitToolbarItems();
        }
    }
}