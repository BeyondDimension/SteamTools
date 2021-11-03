using System;
using System.Application.UI.ViewModels;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using VM = System.Application.UI.ViewModels.MyPageViewModel;

namespace System.Application.UI.Views
{
    public partial class MyPage : BaseContentPage<VM>
    {
        public MyPage()
        {
            InitializeComponent();
            ViewModel = VM.Instance;
        }
    }
}