using System.Reactive.Disposables;

// ReSharper disable once CheckNamespace
namespace System;

public static partial class CompositeDisposableExtensions
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Add(this CompositeDisposable @this, Action dispose)
        => @this.Add(Disposable.Create(dispose));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Add(this CompositeDisposable @this, Func<ValueTask> dispose)
        => @this.Add(new AsyncDisposable(dispose));

#if DEBUG
    [Obsolete("use ValueTask", true)]
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Add(this CompositeDisposable @this, Func<Task> dispose)
        => @this.Add(Disposable.Create(dispose.RunSync));
#endif

    sealed class AsyncDisposable : IDisposable, IAsyncDisposable
    {
        // https://learn.microsoft.com/zh-cn/dotnet/standard/garbage-collection/implementing-disposeasync#implement-both-dispose-and-async-dispose-patterns

        IDisposable? _disposableResource;
        IAsyncDisposable? _asyncDisposableResource;

        public AsyncDisposable(IDisposable? disposableResource)
        {
            _disposableResource = disposableResource;
        }

        public AsyncDisposable(IAsyncDisposable? asyncDisposableResource)
        {
            _asyncDisposableResource = asyncDisposableResource;
        }

        public AsyncDisposable(Func<ValueTask>? dispose) : this(Nito.Disposables.AnonymousAsyncDisposable.Create(dispose))
        {

        }

        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        public async ValueTask DisposeAsync()
        {
            await DisposeAsyncCoreAsync().ConfigureAwait(false);

            Dispose(disposing: false);
            GC.SuppressFinalize(this);
        }

        void Dispose(bool disposing)
        {
            if (disposing)
            {
                _disposableResource?.Dispose();
                (_asyncDisposableResource as IDisposable)?.Dispose();
                _disposableResource = null;
                _asyncDisposableResource = null;
            }
        }

        async ValueTask DisposeAsyncCoreAsync()
        {
            if (_asyncDisposableResource is not null)
            {
                await _asyncDisposableResource.DisposeAsync().ConfigureAwait(false);
            }

            if (_disposableResource is IAsyncDisposable disposable)
            {
                await disposable.DisposeAsync().ConfigureAwait(false);
            }
            else
            {
                _disposableResource?.Dispose();
            }

            _asyncDisposableResource = null;
            _disposableResource = null;
        }
    }
}