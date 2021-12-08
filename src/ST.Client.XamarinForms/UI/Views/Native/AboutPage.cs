using System;
using System.Application.Services;
using System.Collections.Generic;
using System.Text;
using VM = System.Application.UI.ViewModels.AboutPageViewModel;

namespace System.Application.UI.Views.Native
{
    public class AboutPage : BaseContentPage<VM>
    {
        public AboutPage()
        {
            ViewModel = VM.Instance;
        }
    }
}
