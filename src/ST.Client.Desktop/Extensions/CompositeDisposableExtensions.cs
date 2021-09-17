using Nito.Disposables;
using System.Reactive.Disposables;

// ReSharper disable once CheckNamespace
namespace System
{
    public static class CompositeDisposableExtensions
    {
        public static void Add(this CompositeDisposable @this, Action releaseAction)
        {
            AnonymousDisposable disposable = new(releaseAction);
            @this.Add(disposable);
        }
    }
}
