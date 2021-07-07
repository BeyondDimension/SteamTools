using ReactiveUI;
using System.Application.Services;
using System.Threading.Tasks;
using System.Windows;

namespace System.Application.UI.ViewModels
{
    public class TextBoxWindowViewModel : PageViewModel, ITextBoxWindowViewModel
    {
        private string? _Value;
        public string? Value
        {
            get => _Value;
            set => this.RaiseAndSetIfChanged(ref _Value, value);
        }

        public string? Placeholder { get; set; }

        public static async Task<string?> ShowDialog(TextBoxWindowViewModel? vm = null)
        {
            vm ??= new TextBoxWindowViewModel();
            await IShowWindowService.Instance.ShowDialog(CustomWindow.TextBox, vm, string.Empty, ResizeModeCompat.CanResize);
            return vm.Value;
        }
    }
}