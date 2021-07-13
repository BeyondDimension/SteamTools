using ReactiveUI;
using System.Runtime.Serialization;

// ReSharper disable once CheckNamespace
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

#if DEBUG
        [Obsolete("use System.Application.MainThread2.BeginInvokeOnMainThread", true)]
        protected void InvokeOnUIDispatcher(Action action)
        {
            MainThread2.BeginInvokeOnMainThread(action);
        }
#endif
    }
}