using Android.Util;
using Android.Views;
using AndroidX.AppCompat.Widget;
using Binding;
using ReactiveUI;
using System.Application.Services;
using System.Application.Settings;
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

            SetConsoleFontSize(ASFSettings.ConsoleFontSize.Value);
            ASFSettings.ConsoleFontSize.ValueChanged += ConsoleFontSize_ValueChanged;
        }

        public override void OnDestroyView()
        {
            ASFSettings.ConsoleFontSize.ValueChanged -= ConsoleFontSize_ValueChanged;
            base.OnDestroyView();
        }

        void SetConsoleFontSize(int value)
        {
            if (binding == null) return;
            binding.tvConsole.SetTextSize(ComplexUnitType.Sp, value);
            binding.tvArrow.SetTextSize(ComplexUnitType.Sp, value);
            binding.tbInput.SetTextSize(ComplexUnitType.Sp, value);
        }

        void ConsoleFontSize_ValueChanged(object sender, ValueChangedEventArgs<int> e)
        {
            SetConsoleFontSize(e.NewValue);
        }
    }
}
