using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Platform;
using Avalonia.Shared.PlatformSupport;
using Avalonia.Threading;
using ReactiveUI;
using System.Application.Mvvm;
using System.Application.Services;
using System.Application.UI.Resx;
using System.Application.UI.ViewModels;
using System.Application.UI.Views.Windows;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using AvaloniaApplication = Avalonia.Application;
#if LINUX
using GtkApplication = Gtk.Application;
#endif

namespace System.Application.UI
{
    static class NotifyIconHelper
    {
        static object GetIcon(IAssetLoader assets)
        {
            string iconPath;
            if (OperatingSystem2.IsMacOS)
            {
                iconPath = "avares://System.Application.SteamTools.Client.Desktop.Avalonia/Application/UI/Assets/Icon_16.png";
            }
            else
            {
                iconPath = "avares://System.Application.SteamTools.Client.Desktop.Avalonia/Application/UI/Assets/Icon.ico";
            }
            return assets.Open(new(iconPath));
        }

        public static object GetIconByCurrentAvaloniaLocator()
        {
            var assets = AvaloniaLocator.Current.GetService<IAssetLoader>();
            return GetIcon(assets);
        }

        public static object GetIcon()
        {
            var assets = new AssetLoader(typeof(TaskBarWindow).Assembly);
            return GetIcon(assets);
        }

        public static (NotifyIcon notifyIcon, IDisposable? menuItemDisposable) Init(Func<object> getIcon)
        {
            IDisposable? menuItemDisposable = null;
            var notifyIcon = DI.Get<NotifyIcon>();
            notifyIcon.Text = TaskBarWindowViewModel.TitleString;
            var assets = AvaloniaLocator.Current.GetService<IAssetLoader>();
            string iconPath;
            if (OperatingSystem2.IsMacOS)
            {
                iconPath = "avares://System.Application.SteamTools.Client.Desktop.Avalonia/Application/UI/Assets/Icon_16.png";
            }
            else
            {
                iconPath = "avares://System.Application.SteamTools.Client.Desktop.Avalonia/Application/UI/Assets/Icon.ico";
            }
            notifyIcon.Icon = assets.Open(new(iconPath));
#if WINDOWS
            notifyIcon.RightClick += (_, e) =>
            {
                IWindowService.Instance.ShowTaskBarWindow(e.X, e.Y);
            };
#else
            menuItemDisposable = InitMenuItems(notifyIcon);
#endif
            return (notifyIcon, menuItemDisposable);
        }

        #region 仅在非 Windows 上使用平台原生托盘菜单
#if !WINDOWS || DEBUG
        static string Exit => AppResources.Exit;

        static ContextMenuStrip.MenuItem? exitMenuItem;
        static readonly Dictionary<MainWindowViewModel.ITabItemViewModel, ContextMenuStrip.MenuItem> tabItems = new();
        static IDisposable? InitMenuItems(NotifyIcon notifyIcon)
        {
            if (IWindowService.Instance.MainWindow is MainWindowViewModel main)
            {
                var query = from x in main.TabItems.Concat(main.FooterTabItems)
                            let tabItem = x is MainWindowViewModel.ITabItemViewModel item ? item : null
                            where tabItem != null
                            select tabItem;
                foreach (var item in query)
                {
                    var menuItem = new ContextMenuStrip.MenuItem
                    {
                        Text = item.Name,
                        Command = ReactiveCommand.Create(() =>
                        {
                            TaskBarWindowViewModel.OnMenuClick(item.Id);
                        }),
                    };
                    tabItems.Add(item, menuItem);
                    notifyIcon.ContextMenuStrip.Items.Add(menuItem);
                }
                exitMenuItem = new ContextMenuStrip.MenuItem
                {
                    Text = Exit,
                    Command = ReactiveCommand.Create(() =>
                    {
                        TaskBarWindowViewModel.OnMenuClick(TaskBarWindowViewModel.CommandExit);
                    }),
                };
                notifyIcon.ContextMenuStrip.Items.Add(exitMenuItem);

                return R.Current.WhenAnyValue(x => x.Res).SubscribeInMainThread(_ =>
                {
                    if (exitMenuItem != null)
                    {
                        exitMenuItem.Text = Exit;
                    }
                    foreach (var item in tabItems)
                    {
                        item.Value.Text = item.Key.Name;
                    }
                });
            }

            return null;
        }
#endif
        #endregion

        #region 仅在 Linux 上使用管道 IPC 调用托盘菜单项点击事件
#if LINUX || DEBUG
        public abstract class PipeCore : IDisposable
        {
            public const string CommandNotifyIconClick = "NotifyIconClick";

            protected readonly CancellationTokenSource cts = new();

            bool isStarted;

            /// <summary>
            ///
            /// </summary>
            public void OnStart()
            {
                if (isStarted) return;
                isStarted = true;
                Task.Run(OnStartCore);
            }

            protected abstract void OnStartCore();

            bool disposedValue;
            protected virtual void Dispose(bool disposing)
            {
                if (!disposedValue)
                {
                    if (disposing)
                    {
                        // TODO: 释放托管状态(托管对象)
                        cts.Cancel();
                    }

                    // TODO: 释放未托管的资源(未托管的对象)并重写终结器
                    // TODO: 将大型字段设置为 null
                    disposedValue = true;
                }
            }

            // // TODO: 仅当“Dispose(bool disposing)”拥有用于释放未托管资源的代码时才替代终结器
            // ~NotifyIconPipeCore()
            // {
            //     // 不要更改此代码。请将清理代码放入“Dispose(bool disposing)”方法中
            //     Dispose(disposing: false);
            // }

            public void Dispose()
            {
                // 不要更改此代码。请将清理代码放入“Dispose(bool disposing)”方法中
                Dispose(disposing: true);
                GC.SuppressFinalize(this);
            }
        }

        public sealed class PipeServer : PipeCore
        {
            /// <summary>
            ///
            /// </summary>
            /// <param name="handle"></param>
            /// <returns></returns>
            static string GetArguments(string handle)
                => $"-clt tray -handle {handle} -pid {Environment.ProcessId}";

            bool HandlerCommand(string command)
            {
                switch (command)
                {
                    case CommandNotifyIconClick:
                        Dispatcher.UIThread.Post(() =>
                        {
                            App.Instance.NotifyIcon_Click(this, EventArgs.Empty);
                        }, DispatcherPriority.MaxValue);
                        break;
                    default:
                        return TaskBarWindowViewModel.OnMenuClick(command);
                }
                return default;
            }

            protected override void OnStartCore()
            {
                Process pipeClient = new();

                pipeClient.StartInfo.FileName = AppHelper.ProgramPath;

                using (var pipeServer = new AnonymousPipeServerStream(PipeDirection.In, HandleInheritability.Inheritable))
                {
                    // Pass the client process a handle to the server.
                    pipeClient.StartInfo.Arguments =
                        GetArguments(pipeServer.GetClientHandleAsString());
                    pipeClient.StartInfo.UseShellExecute = false;
                    pipeClient.Start();

                    pipeServer.DisposeLocalCopyOfClientHandle();

                    try
                    {
                        // Read user input and send that to the client process.
                        using var sr = new StreamReader(pipeServer);

                        // Display the read text to the console
                        string? command;

                        // Read the server data and echo to the console.
                        while ((command = sr.ReadLine()) != null)
                        {
                            try
                            {
                                cts.Token.ThrowIfCancellationRequested();

                                if (HandlerCommand(command)) break;
                            }
                            catch (OperationCanceledException)
                            {
                                break;
                            }
                        }
                    }
                    // Catch the IOException that is raised if the pipe is broken
                    // or disconnected.
                    catch (IOException)
                    {
                    }
                }

                pipeClient.WaitForExit();
                pipeClient.Close();
            }
        }

        public sealed class PipeClient : PipeCore
        {
            Process? mainProcess;
            readonly string pipeHandleAsString;
            readonly ConcurrentQueue<string> queue = new();

            public PipeClient(string pipeHandleAsString, int mainProcessId)
            {
                this.pipeHandleAsString = pipeHandleAsString;
                mainProcess = Process.GetProcessById(mainProcessId);
                mainProcess.Exited += MainProcess_Exited;
            }

            private void MainProcess_Exited(object? sender, EventArgs e) => Dispose();

            protected override void OnStartCore()
            {
                try
                {
                    using var pipeClient = new AnonymousPipeClientStream(PipeDirection.Out, pipeHandleAsString);
                    using var sw = new StreamWriter(pipeClient);
                    sw.AutoFlush = true;

                    while (true)
                    {
                        try
                        {
                            cts.Token.ThrowIfCancellationRequested();

                            if (mainProcess == null || mainProcess.HasExited) break;

                            if (queue.TryDequeue(out var result))
                            {
                                sw.WriteLine(result);
                                if (result == TaskBarWindowViewModel.CommandExit) break;
                            }
                        }
                        catch (OperationCanceledException)
                        {
                            sw.WriteLine(TaskBarWindowViewModel.CommandExit);
                            break;
                        }
                    }
                }
                catch (Exception)
                {
                }

                Dispose();
            }

            /// <summary>
            ///
            /// </summary>
            /// <param name="value"></param>
            public void Append(string value)
            {
                if (!disposedValue && !cts.IsCancellationRequested)
                {
                    queue.Enqueue(value);
                }
            }

            bool disposedValue;
            protected override void Dispose(bool disposing)
            {
                if (!disposedValue)
                {
                    if (disposing)
                    {
                        // TODO: 释放托管状态(托管对象)
                        if (mainProcess != null)
                        {
                            mainProcess.Exited -= MainProcess_Exited;
                            mainProcess = null;
                        }

                        DI.Get<NotifyIcon>().Dispose();

#if LINUX
                        GtkApplication.Quit();
#endif
                        if (OperatingSystem2.IsWindows)
                        {
                            if (AvaloniaApplication.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
                            {
                                Dispatcher.UIThread.Post(() =>
                                {
                                    desktop.Shutdown();
                                }, DispatcherPriority.MaxValue);
                            }
                        }
                    }

                    // TODO: 释放未托管的资源(未托管的对象)并重写终结器
                    // TODO: 将大型字段设置为 null
                    disposedValue = true;
                }
                base.Dispose(disposing);
            }
        }
#endif
        #endregion
    }
}