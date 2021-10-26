using Android.Views;
using AndroidX.AppCompat.Widget;
using Binding;
using ReactiveUI;
using System.Application.Services;
using System.Application.UI.Resx;
using System.Application.UI.ViewModels;
using System.Text;
using static System.Application.UI.Resx.AppResources;

// ReSharper disable once CheckNamespace
namespace System.Application.UI.Fragments
{
    internal sealed class ASFPlusConsoleFragment : ASFPlusFragment<fragment_asf_plus_console>
    {
        protected override int? LayoutResource => Resource.Layout.fragment_asf_plus_console;

        public override void OnCreateView(View view)
        {
            base.OnCreateView(view);

            ASFService.Current.WhenAnyValue(x => x.ConsoleLogText).SubscribeInMainThread(value =>
            {
                if (binding == null) return;
                binding.tvConsole.Text = value;
            }).AddTo(this);
        }
    }
}
