using System.Windows.Input;

namespace System.Application.UI.ViewModels
{
    public class DialogWindowViewModel : WindowViewModel
    {
        public bool DialogResult { get; set; }

        public ICommand? OK { get; set; }

        public ICommand? Cancel { get; set; }
    }
}