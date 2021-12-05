using Android.Util;
using Android.Views;
using Android.Views.InputMethods;
using Android.Widget;
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
    internal sealed class ASFPlusConsoleFragment : ASFPlusFragment<fragment_asf_plus_console>, TextView.IOnEditorActionListener
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

            binding!.tbInput.SetOnEditorActionListener(this);

            binding.tvConsole.TextChanged += (_, _) => ConsoleTextChanged();
            ConsoleTextChanged();
        }

        void ConsoleTextChanged()
        {
            if (binding == null) return;
            if (string.IsNullOrWhiteSpace(binding.tvConsole.Text))
            {
                if (binding.tvConsole.Visibility != ViewStates.Gone)
                {
                    binding.tvConsole.Visibility = ViewStates.Gone;
                }
            }
            else
            {
                if (binding.tvConsole.Visibility != ViewStates.Visible)
                {
                    binding.tvConsole.Visibility = ViewStates.Visible;
                }
            }
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

        void CommandSubmit()
        {
            var command = binding!.tbInput.Text?.Trim();
            if (!string.IsNullOrEmpty(command))
            {
                binding.tbInput.Text = string.Empty;
                IArchiSteamFarmService.Instance.CommandSubmit(command!);
            }
        }

        bool TextView.IOnEditorActionListener.OnEditorAction(TextView? view, ImeAction actionId, KeyEvent? e)
        {
            if (view != null && binding != null && ((e != null && e.Action == KeyEventActions.Down && e.KeyCode == Keycode.Enter) || e == null))
            {
                if (view.Id == Resource.Id.tbInput)
                {
                    CommandSubmit();
                    return true;
                }
            }
            return false;
        }
    }
}
