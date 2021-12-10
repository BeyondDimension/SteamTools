using System;
using System.Application.Services;
using System.Application.UI.ViewModels;
using System.Collections.Generic;
using System.Text;
using VM = System.Application.UI.ViewModels.MyPageViewModel;

namespace System.Application.UI.Views.Native
{
    public partial class MyPage : BaseContentPage<VM>
    {
        public MyPage()
        {
            ViewModel = VM.Instance;
        }
    }
}
