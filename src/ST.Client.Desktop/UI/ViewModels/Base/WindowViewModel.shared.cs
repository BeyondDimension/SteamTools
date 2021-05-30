using ReactiveUI;
using System.Runtime.Serialization;
#if __MOBILE__
using Xamarin.Essentials;
#else
using MainThread = System.Application.MainThreadDesktop;
#endif

namespace System.Application.UI.ViewModels
{
    public partial class
#if __MOBILE__
        PageViewModel
#else
        WindowViewModel
#endif
        : ViewModelBase
    {
        string title = string.Empty;
        [IgnoreDataMember]
        public string Title
        {
            get => title;
            set => this.RaiseAndSetIfChanged(ref title, value);
        }

#if __MOBILE__
        [Obsolete("use Xamarin.Essentials.MainThread.BeginInvokeOnMainThread")]
#else
        [Obsolete("use System.Application.MainThreadDesktop.BeginInvokeOnMainThread")]
#endif
        protected void InvokeOnUIDispatcher(Action action)
        {
            MainThread.BeginInvokeOnMainThread(action);
        }
    }
}