using System.Reactive.Disposables;
using System.Threading.Tasks;

// ReSharper disable once CheckNamespace
namespace System
{
    public static partial class CompositeDisposableExtensions
    {
        public static void Add(this CompositeDisposable @this, Action releaseAction)
        {
            @this.Add(Disposable.Create(releaseAction));
        }

        public static void Add(this CompositeDisposable @this, Func<Task> asyncFunc)
        {
            @this.Add(Disposable.Create(asyncFunc.RunSync));
        }
    }
}
