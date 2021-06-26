using Android.Runtime;
using Android.Views;
using Binding;
using System.Application.UI.Resx;
using System.Application.UI.ViewModels;
using System.Properties;
using System.Windows;

namespace System.Application.UI.Fragments
{
    internal sealed class UserBasicInfoFragment : BaseFragment<fragment_basic_profile, UserProfilePageViewModel>
    {
        protected override int? LayoutResource => Resource.Layout.fragment_basic_profile;

        public override void OnCreateView(View view)
        {
            base.OnCreateView(view);
        }
    }
}
