using Android.Views;
using Binding;
using ReactiveUI;
using System.Application.UI.Resx;
using System.Application.UI.ViewModels;
using static System.Application.UI.Resx.AppResources;

// ReSharper disable once CheckNamespace
namespace System.Application.UI.Fragments
{
    internal sealed class ASFPlusBotFragment : ASFPlusFragment<fragment_asf_plus_bot>
    {
        protected override int? LayoutResource => Resource.Layout.fragment_asf_plus_bot;

        public override void OnCreateView(View view)
        {
            base.OnCreateView(view);
        }
    }
}
