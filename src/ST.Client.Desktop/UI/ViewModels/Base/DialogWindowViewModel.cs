using System.Windows.Input;
#if __MOBILE__
using WindowViewModel = System.Application.UI.ViewModels.PageViewModel;
#endif

namespace System.Application.UI.ViewModels
{
    public class DialogWindowViewModel : WindowViewModel
    {
        public bool DialogResult { get; set; }

        public ICommand? OK { get; set; }

        public ICommand? Cancel { get; set; }
    }
}