// https://github.com/AvaloniaUI/Avalonia/blob/e1138f2cb6a393802b235a073d28e85a64690ffe/src/Avalonia.Controls/ApplicationLifetimes/ClassicDesktopStyleApplicationLifetime.cs

using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Interactivity;
using Avalonia.Platform;
using Avalonia.Threading;

namespace Avalonia.Controls.ApplicationLifetimes
{
    sealed class ClassicDesktopStyleApplicationLifetime2 : IClassicDesktopStyleApplicationLifetime, IDisposable
    {
        int _exitCode;
        CancellationTokenSource? _cts;
        bool _isShuttingDown;
        readonly HashSet<Window> _windows = new();

#pragma warning disable SA1308 // Variable names should not be prefixed
        static ClassicDesktopStyleApplicationLifetime2? s_activeLifetime;
#pragma warning restore SA1308 // Variable names should not be prefixed

        static ClassicDesktopStyleApplicationLifetime2()
        {
            Window.WindowOpenedEvent.AddClassHandler(typeof(Window), OnWindowOpened);
            Window.WindowClosedEvent.AddClassHandler(typeof(Window), OnWindowClosed);
        }

        static void OnWindowClosed(object? sender, RoutedEventArgs e)
        {
            var window = (Window)sender!;
            s_activeLifetime?._windows.Remove(window);
            s_activeLifetime?.HandleWindowClosed(window);
        }

        static void OnWindowOpened(object? sender, RoutedEventArgs e)
        {
            s_activeLifetime?._windows.Add((Window)sender!);
        }

        public ClassicDesktopStyleApplicationLifetime2()
        {
            if (s_activeLifetime != null)
                throw new InvalidOperationException(
                    "Can not have multiple active ClassicDesktopStyleApplicationLifetime instances and the previously created one was not disposed");
            s_activeLifetime = this;
        }

        /// <inheritdoc/>
        public event EventHandler<ControlledApplicationLifetimeStartupEventArgs>? Startup;

        /// <inheritdoc/>
        public event EventHandler<ShutdownRequestedEventArgs>? ShutdownRequested;

        /// <inheritdoc/>
        public event EventHandler<ControlledApplicationLifetimeExitEventArgs>? Exit;

        /// <summary>
        /// Gets the arguments passed to the AppBuilder Start method.
        /// </summary>
        public string[]? Args { get; set; }

        /// <inheritdoc/>
        public ShutdownMode ShutdownMode { get; set; }

        /// <inheritdoc/>
        public Window? MainWindow { get; set; }

        /// <inheritdoc />
        public IReadOnlyList<Window> Windows => _windows.ToArray();

        void HandleWindowClosed(Window? window)
        {
            if (window == null)
                return;

            if (_isShuttingDown)
                return;

            if (ShutdownMode == ShutdownMode.OnLastWindowClose && _windows.Count == 0)
                TryShutdown();
            else if (ShutdownMode == ShutdownMode.OnMainWindowClose && ReferenceEquals(window, MainWindow))
                TryShutdown();
        }

        public void Shutdown(int exitCode = 0)
        {
            DoShutdown(new ShutdownRequestedEventArgs(), true, true, exitCode);
        }

        public bool TryShutdown(int exitCode = 0)
        {
            return DoShutdown(new ShutdownRequestedEventArgs(), true, false, exitCode);
        }

        public int Start(string[] args)
        {
            Startup?.Invoke(this, new ControlledApplicationLifetimeStartupEventArgs(args));

            var options = AvaloniaLocator.Current.GetService<ClassicDesktopStyleApplicationLifetimeOptions>();

            if (options != null && options.ProcessUrlActivationCommandLine && args.Length > 0)
            {
                if (Application.Current is IApplicationPlatformEvents events)
                {
                    events.RaiseUrlsOpened(args);
                }
            }

            var lifetimeEvents = AvaloniaLocator.Current.GetService<IPlatformLifetimeEventsImpl>();

            if (lifetimeEvents != null)
                lifetimeEvents.SetShutdownRequested(OnShutdownRequested);

            _cts = new CancellationTokenSource();

            // Note due to a bug in the JIT we wrap this in a method, otherwise MainWindow
            // gets stuffed into a local var and can not be GCed until after the program stops.
            // this method never exits until program end.
            ShowMainWindow();

            Dispatcher.UIThread.MainLoop(_cts.Token);
            Environment.ExitCode = _exitCode;
            return _exitCode;
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        void ShowMainWindow()
        {
            MainWindow?.Show();
        }

        public void Dispose()
        {
            if (s_activeLifetime == this)
                s_activeLifetime = null;
        }

        bool DoShutdown(
             ShutdownRequestedEventArgs e,
             bool isProgrammatic,
             bool force = false,
             int exitCode = 0)
        {
            if (!force)
            {
                ShutdownRequested?.Invoke(this, e);

                if (e.Cancel)
                    return false;

                // https://appcenter.ms/orgs/BeyondDimension/apps/Steam/crashes/errors/3780037517u/overview
                // ClassicDesktopStyleApplicationLifetime.DoShutdown (ShutdownRequestedEventArgs e, Boolean force, Int32 exitCode) /_/src/Avalonia.Controls/ApplicationLifetimes/ClassicDesktopStyleApplicationLifetime.cs, line 147
                // System.InvalidOperationException: Application is already shutting down.
                // v2.8.2 | 73 users | 76 reports
                // Exception Stack:
                // System.InvalidOperationException: Application is already shutting down.
                //   at Avalonia.Controls.ApplicationLifetimes.ClassicDesktopStyleApplicationLifetime.DoShutdown(ShutdownRequestedEventArgs e, Boolean force, Int32 exitCode) in /_/src/Avalonia.Controls/ApplicationLifetimes/ClassicDesktopStyleApplicationLifetime.cs:line 147
                //   at Avalonia.Controls.ApplicationLifetimes.ClassicDesktopStyleApplicationLifetime.OnShutdownRequested(Object sender, ShutdownRequestedEventArgs e) in /_/src/Avalonia.Controls/ApplicationLifetimes/ClassicDesktopStyleApplicationLifetime.cs:line 186
                //   at Avalonia.Win32.Win32Platform.WndProc(IntPtr hWnd, UInt32 msg, IntPtr wParam, IntPtr lParam) in /_/src/Windows/Avalonia.Win32/Win32Platform.cs:line 283
                if (_isShuttingDown)
                    //    throw new InvalidOperationException("Application is already shutting down.");
                    return true;
            }

            _exitCode = exitCode;
            _isShuttingDown = true;

            try
            {
                // When an OS shutdown request is received, try to close all non-owned windows. Windows can cancel
                // shutdown by setting e.Cancel = true in the Closing event. Owned windows will be shutdown by their
                // owners.
                foreach (var w in Windows)
                {
                    if (w.Owner is null)
                    {
                        w.CloseCore_(WindowCloseReason.ApplicationShutdown, isProgrammatic, false);
                    }
                }

                if (!force && Windows.Count > 0)
                {
                    e.Cancel = true;
                    return false;
                }

                var args = new ControlledApplicationLifetimeExitEventArgs(exitCode);
                Exit?.Invoke(this, args);
                _exitCode = args.ApplicationExitCode;
            }
            finally
            {
                _cts?.Cancel();
                _cts = null;
                _isShuttingDown = false;
            }

            return true;
        }

        void OnShutdownRequested(object? sender, ShutdownRequestedEventArgs e)
            => DoShutdown(e, false);
    }
}

namespace Avalonia
{
    public static class ClassicDesktopStyleApplicationLifetimeExtensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int StartWithClassicDesktopLifetime2(
            this AppBuilder builder, string[] args, ShutdownMode shutdownMode = ShutdownMode.OnLastWindowClose)
        {
            var lifetime = new ClassicDesktopStyleApplicationLifetime2()
            {
                Args = args,
                ShutdownMode = shutdownMode,
            };
            builder.SetupWithLifetime(lifetime);
            return lifetime.Start(args);
        }
    }
}