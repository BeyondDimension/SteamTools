#if !TRAY_INDEPENDENT_PROGRAM
using Avalonia;
using Avalonia.Platform;
using Avalonia.Shared.PlatformSupport;
using System.Application.Services;
using System.Application.UI.Views.Windows;
#else
using System.Application.UI.Properties;
#endif
using ReactiveUI;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Application.UI.ViewModels;
using System.Application.UI.Resx;
using Avalonia.Controls;
//#if LINUX && TRAY_INDEPENDENT_PROGRAM
//using GtkApplication = Gtk.Application;
//#endif

namespace System.Application.UI
{
    /// <inheritdoc cref="INotificationService.NotifyIconHelper"/>
    sealed class NotifyIconHelper : INotificationService.NotifyIconHelper
    {
        private NotifyIconHelper() => throw new NotSupportedException();

#if !TRAY_INDEPENDENT_PROGRAM
        static Stream GetIcon(IAssetLoader assets)
        {
            string iconPath;
            if (OperatingSystem2.IsMacOS)
            {
                iconPath = "avares://System.Application.SteamTools.Client.Avalonia/Application/UI/Assets/Icon_16.png";
            }
            else
            {
                iconPath = "avares://System.Application.SteamTools.Client.Avalonia/Application/UI/Assets/Icon.ico";
            }
            return assets.Open(new(iconPath));
        }

        static Stream GetIconByCurrentAvaloniaLocator()
        {
            var assets = AvaloniaLocator.Current.GetService<IAssetLoader>()!;
            return GetIcon(assets);
        }

        public static NotifyIcon? Init(App app, EventHandler notifyIconClick)
        {
            if (IsInitialized) return null;
            NotifyIcon? notifyIcon = null;
            IDisposable? menuItemDisposable = null;
            var icon = GetIconByCurrentAvaloniaLocator();
            var text = TaskBarWindowViewModel.TitleString;
#if WINDOWS
            if (OperatingSystem2.IsWindows)
            {
                notifyIcon = DI.Get<NotifyIcon>();
                notifyIcon.Text = text;
                notifyIcon.Icon = icon;
#if WINDOWS
                notifyIcon.RightClick += (_, e) =>
                {
                    IViewModelManager.Instance.ShowTaskBarWindow(e.X, e.Y);
                };
#else
                menuItemDisposable = InitMenuItems(notifyIcon);
#endif
                notifyIcon.Click += notifyIconClick;
                notifyIcon.DoubleClick += notifyIconClick;
                notifyIcon.AddTo(app);
            }
            else
#endif
            {
                //                if (OperatingSystem2.IsLinux)
                //                {
                ////#pragma warning disable CA1416 // 验证平台兼容性
                ////                    if (IPlatformService.Instance.IsDeepin)
                ////#pragma warning restore CA1416 // 验证平台兼容性
                ////                    {
                ////                        // Deepin catch.
                ////                        // Unhandled exception: System.Reflection.TargetInvocationException: Arg_TargetInvocationException
                ////                        //---> Tmds.DBus.DBusException: com.deepin.DBus.Error.Unnamed: notifier item has been registered
                ////                        //  at Tmds.DBus.DBusConnection.CallMethodAsync(Message msg, Boolean checkConnected, Boolean checkReplyType)
                ////                        //  at Tmds.DBus.Connection.CallMethodAsync(Message message)
                ////                        //  at Tmds.DBus.CodeGen.DBusObjectProxy.SendMethodReturnReaderAsync(String iface, String member, Nullable`1 inSignature, MessageWriter writer)
                ////                        //  at Avalonia.X11.X11TrayIconImpl.CreateTrayIcon() in /_/src/Avalonia.X11/X11TrayIconImpl.cs:line 85
                ////                        //  at System.Threading.Tasks.Task.<>c.<ThrowAsync>b__127_0(Object state)
                ////                        //  at Avalonia.Threading.AvaloniaSynchronizationContext.<>c__DisplayClass5_0.<Post>b__0() in /_/src/Avalonia.Base/Threading/AvaloniaSynchronizationContext.cs:line 33
                ////                        //  at Avalonia.Threading.JobRunner.Job.Avalonia.Threading.JobRunner.IJob.Run() in /_/src/Avalonia.Base/Threading/JobRunner.cs:line 166
                ////                        //  at Avalonia.Threading.JobRunner.RunJobs(Nullable`1 priority) in /_/src/Avalonia.Base/Threading/JobRunner.cs:line 37
                ////                        //  at Avalonia.X11.X11PlatformThreading.CheckSignaled() in /_/src/Avalonia.X11/X11PlatformThreading.cs:line 164
                ////                        //  at Avalonia.X11.X11PlatformThreading.RunLoop(CancellationToken cancellationToken) in /_/src/Avalonia.X11/X11PlatformThreading.cs:line 244
                ////                        //  at Avalonia.Threading.Dispatcher.MainLoop(CancellationToken cancellationToken) in /_/src/Avalonia.Base/Threading/Dispatcher.cs:line 61
                ////                        return null;
                ////                    }
                //                }
                NativeMenu menu = new();
                menuItemDisposable = InitMenuItems(menu);
                TrayIcon trayIcon = new()
                {
                    Icon = new(icon),
                    ToolTipText = text,
                    Menu = menu,
                };
                trayIcon.Clicked += notifyIconClick;
                TrayIcon.SetIcons(app, new()
                {
                    trayIcon,
                });
                if (OperatingSystem2.IsMacOS)
                {
                    NativeMenu.SetMenu(app, menu);
                }
            }
            if (menuItemDisposable != null) menuItemDisposable.AddTo(app);
            IsInitialized = true;
            return notifyIcon;
        }

        public static
#if !TRAY_INDEPENDENT_PROGRAM
            Stream
#else
            object
#endif
            GetIcon()
        {
#if !TRAY_INDEPENDENT_PROGRAM
            var assets = new AssetLoader(typeof(TaskBarWindow).Assembly);
            return GetIcon(assets);
#else
            return SR.Icon;
#endif
        }
#endif

        //        public static (NotifyIcon notifyIcon, IDisposable? menuItemDisposable) Init(Func<object> getIcon)
        //        {
        //            IDisposable? menuItemDisposable = null;
        //            var notifyIcon = DI.Get<NotifyIcon>();
        //            notifyIcon.Text = TaskBarWindowViewModel.TitleString;
        //            notifyIcon.Icon = getIcon();
        //#if WINDOWS
        //            notifyIcon.RightClick += (_, e) =>
        //            {
        //                IViewModelManager.Instance.ShowTaskBarWindow(e.X, e.Y);
        //            };
        //#else
        //            menuItemDisposable = InitMenuItems(notifyIcon);
        //#endif
        //            return (notifyIcon, menuItemDisposable);
        //        }

        #region 仅在非 Windows 上使用平台原生托盘菜单
        static string Exit => AppResources.Exit;

        static void OnMenuClick(string command)
        {
#if TRAY_INDEPENDENT_PROGRAM
            notifyIconPipeClient?.Append(command);
#else
            TaskBarWindowViewModel.OnMenuClick(command);
#endif
        }

        static void OnMenuClick(TabItemViewModel.TabItemId tabItemId)
        {
#if TRAY_INDEPENDENT_PROGRAM
            notifyIconPipeClient?.Append(tabItemId.ToString());
#else
            TaskBarWindowViewModel.OnMenuClick(tabItemId);
#endif
        }

        //        static ContextMenuStrip.MenuItem? exitMenuItem;
        //        static readonly Dictionary<TabItemViewModel, ContextMenuStrip.MenuItem> tabItems = new();
        //        static IDisposable? InitMenuItems(NotifyIcon notifyIcon)
        //        {
        //#if !TRAY_INDEPENDENT_PROGRAM
        //            if (IViewModelManager.Instance.MainWindow is not MainWindowViewModel main) return null;
        //#else
        //            MainWindowViewModel main = new();
        //#endif
        //            var query = from x in main.AllTabItems
        //                        let tabItem = x is TabItemViewModel item ? item : null
        //                        where tabItem != null
        //                        select tabItem;
        //            foreach (var item in query)
        //            {
        //                var menuItem = new ContextMenuStrip.MenuItem
        //                {
        //                    Text = item.Name,
        //                    Command = ReactiveCommand.Create(() =>
        //                    {
        //                        OnMenuClick(item.Id);
        //                    }),
        //                };
        //                tabItems.Add(item, menuItem);
        //                notifyIcon.ContextMenuStrip.Items.Add(menuItem);
        //            }
        //            exitMenuItem = new ContextMenuStrip.MenuItem
        //            {
        //                Text = Exit,
        //                Command = ReactiveCommand.Create(() =>
        //                {
        //                    OnMenuClick(TaskBarWindowViewModel.CommandExit);
        //                }),
        //            };
        //            notifyIcon.ContextMenuStrip.Items.Add(exitMenuItem);

        //#if !TRAY_INDEPENDENT_PROGRAM
        //            return R.Subscribe(() =>
        //            {
        //                if (exitMenuItem != null)
        //                {
        //                    exitMenuItem.Text = Exit;
        //                }
        //                foreach (var item in tabItems)
        //                {
        //                    item.Value.Text = item.Key.Name;
        //                }
        //            });
        //#else
        //            return null;
        //#endif
        //        }
        static NativeMenuItem? exitMenuItem;
        static Dictionary<TabItemViewModel, NativeMenuItem>? tabItems;
        static IDisposable? InitMenuItems(NativeMenu menu)
        {
#if !TRAY_INDEPENDENT_PROGRAM
            if (IViewModelManager.Instance.MainWindow is not MainWindowViewModel main) return null;
#else
            MainWindowViewModel main = new();
#endif
            var query = from x in main.AllTabItems
                        let tabItem = x is TabItemViewModel item ? item : null
                        where tabItem != null
                        select tabItem;
            tabItems = new();
            foreach (var item in query)
            {
                var menuItem = new NativeMenuItem
                {
                    Header = item.Name,
                    Command = ReactiveCommand.Create(() =>
                    {
                        OnMenuClick(item.Id);
                    }),
                };
                tabItems.Add(item, menuItem);
                menu.Add(menuItem);
            }
            exitMenuItem = new NativeMenuItem
            {
                Header = Exit,
                Command = ReactiveCommand.Create(() =>
                {
                    OnMenuClick(TaskBarWindowViewModel.CommandExit);
                }),
            };
            menu.Add(exitMenuItem);

#if !TRAY_INDEPENDENT_PROGRAM
            return R.Subscribe(() =>
            {
                if (exitMenuItem != null)
                {
                    exitMenuItem.Header = Exit;
                }
                foreach (var item in tabItems)
                {
                    item.Value.Header = item.Key.Name;
                }
            });
#else
            return null;
#endif
        }
        #endregion

        //        #region 仅在 Linux 上使用管道 IPC 调用托盘菜单项点击事件
        //#if LINUX || DEBUG
        //        public abstract class PipeCore : IDisposable
        //        {
        //            public const string CommandNotifyIconClick = "NotifyIconClick";

        //            protected readonly CancellationTokenSource cts = new();

        //            bool isStarted;

        //            /// <summary>
        //            ///
        //            /// </summary>
        //            public void OnStart()
        //            {
        //                if (isStarted) return;
        //                isStarted = true;
        //                Task.Run(OnStartCore);
        //            }

        //            protected abstract void OnStartCore();

        //            bool disposedValue;
        //            protected virtual void Dispose(bool disposing)
        //            {
        //                if (!disposedValue)
        //                {
        //                    if (disposing)
        //                    {
        //                        // TODO: 释放托管状态(托管对象)
        //                        cts.Cancel();
        //                    }

        //                    // TODO: 释放未托管的资源(未托管的对象)并重写终结器
        //                    // TODO: 将大型字段设置为 null
        //                    disposedValue = true;
        //                }
        //            }

        //            // // TODO: 仅当“Dispose(bool disposing)”拥有用于释放未托管资源的代码时才替代终结器
        //            // ~NotifyIconPipeCore()
        //            // {
        //            //     // 不要更改此代码。请将清理代码放入“Dispose(bool disposing)”方法中
        //            //     Dispose(disposing: false);
        //            // }

        //            public void Dispose()
        //            {
        //                // 不要更改此代码。请将清理代码放入“Dispose(bool disposing)”方法中
        //                Dispose(disposing: true);
        //                GC.SuppressFinalize(this);
        //            }
        //        }

        //#if !TRAY_INDEPENDENT_PROGRAM

        //        static PipeServer? notifyIconPipeServer;

        //        public static void StopPipeServer() => notifyIconPipeServer?.Dispose();

        //        public static void StartPipeServer()
        //        {
        //            StopPipeServer();
        //            notifyIconPipeServer = new();
        //            notifyIconPipeServer.OnStart();
        //        }

        //        public sealed class PipeServer : PipeCore
        //        {
        //            bool HandlerCommand(string command)
        //            {
        //                switch (command)
        //                {
        //                    case CommandNotifyIconClick:
        //                        Dispatcher.UIThread.Post(() =>
        //                        {
        //                            App.Instance.NotifyIcon_Click(this, EventArgs.Empty);
        //                        }, DispatcherPriority.MaxValue);
        //                        break;
        //                    default:
        //                        return TaskBarWindowViewModel.OnMenuClick(command);
        //                }
        //                return default;
        //            }

        //            protected override void OnStartCore()
        //            {
        //                if (!OperatingSystem.IsLinux()) return;
        //                var fileName = IApplication.ProgramPath + "_LinuxTrayIcon";

        //                var unixSetFileAccessResult = IPlatformService.Instance.UnixSetFileAccess(fileName, IPlatformService.UnixPermission.Combined755);

        //                if (unixSetFileAccessResult != IPlatformService.UnixSetFileAccessResult.Success)
        //                    return;

        //                Process pipeClient = new();

        //                pipeClient.StartInfo.FileName = fileName;

        //                using (var pipeServer = new AnonymousPipeServerStream(PipeDirection.In, HandleInheritability.Inheritable))
        //                {
        //                    // Pass the client process a handle to the server.
        //                    pipeClient.StartInfo.Arguments = pipeServer.GetClientHandleAsString();
        //                    pipeClient.StartInfo.UseShellExecute = false;
        //                    pipeClient.Start();

        //                    pipeServer.DisposeLocalCopyOfClientHandle();

        //                    try
        //                    {
        //                        // Read user input and send that to the client process.
        //                        using var sr = new StreamReader(pipeServer);

        //                        // Display the read text to the console
        //                        string? command;

        //                        // Read the server data and echo to the console.
        //                        while ((command = sr.ReadLine()) != null)
        //                        {
        //                            try
        //                            {
        //                                cts.Token.ThrowIfCancellationRequested();

        //                                if (HandlerCommand(command)) break;
        //                            }
        //                            catch (OperationCanceledException)
        //                            {
        //                                break;
        //                            }
        //                        }
        //                    }
        //                    // Catch the IOException that is raised if the pipe is broken
        //                    // or disconnected.
        //                    catch (IOException)
        //                    {
        //                    }
        //                }

        //                pipeClient.WaitForExit();
        //                pipeClient.Close();
        //            }
        //        }
        //#else

        //        static PipeClient? notifyIconPipeClient;

        //        public static void StopPipeClient() => notifyIconPipeClient?.Dispose();

        //        public static void StartPipeClient(string handle)
        //        {
        //            StopPipeClient();
        //            notifyIconPipeClient = new PipeClient(handle);
        //            notifyIconPipeClient.OnStart();
        //        }

        //        public static PipeClient Client
        //            => notifyIconPipeClient ?? throw new ArgumentNullException(nameof(notifyIconPipeClient));

        //        public sealed class PipeClient : PipeCore
        //        {
        //            Process? mainProcess;
        //            readonly string pipeHandleAsString;
        //            readonly ConcurrentQueue<string> queue = new();

        //            public PipeClient(string pipeHandleAsString)
        //            {
        //                this.pipeHandleAsString = pipeHandleAsString;
        //            }

        //            private void MainProcess_Exited(object? sender, EventArgs e) => Dispose();

        //            protected override void OnStartCore()
        //            {
        //                try
        //                {
        //                    using var pipeClient = new AnonymousPipeClientStream(PipeDirection.Out, pipeHandleAsString);
        //                    using var sw = new StreamWriter(pipeClient);
        //                    sw.AutoFlush = true;

        //                    while (true)
        //                    {
        //                        try
        //                        {
        //                            cts.Token.ThrowIfCancellationRequested();

        //                            if (!pipeClient.IsConnected) break;

        //                            if (queue.TryDequeue(out var result))
        //                            {
        //                                sw.WriteLine(result);
        //                                if (result == TaskBarWindowViewModel.CommandExit) break;
        //                            }
        //                        }
        //                        catch (OperationCanceledException)
        //                        {
        //                            sw.WriteLine(TaskBarWindowViewModel.CommandExit);
        //                            break;
        //                        }
        //                    }
        //                }
        //                catch (Exception)
        //                {
        //                }

        //                Dispose();
        //            }

        //            /// <summary>
        //            ///
        //            /// </summary>
        //            /// <param name="value"></param>
        //            public void Append(string value)
        //            {
        //                if (!disposedValue && !cts.IsCancellationRequested)
        //                {
        //                    queue.Enqueue(value);
        //                }
        //            }

        //            bool disposedValue;
        //            protected override void Dispose(bool disposing)
        //            {
        //                if (!disposedValue)
        //                {
        //                    if (disposing)
        //                    {
        //                        // TODO: 释放托管状态(托管对象)
        //                        if (mainProcess != null)
        //                        {
        //                            mainProcess.Exited -= MainProcess_Exited;
        //                            mainProcess = null;
        //                        }

        //                        DI.Get<NotifyIcon>().Dispose();

        //#if LINUX
        //                        GtkApplication.Quit();
        //#endif

        //#if !TRAY_INDEPENDENT_PROGRAM
        //                        if (OperatingSystem2.IsWindows)
        //                        {
        //                            if (AvaloniaApplication.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        //                            {
        //                                Dispatcher.UIThread.Post(() =>
        //                                {
        //                                    desktop.Shutdown();
        //                                }, DispatcherPriority.MaxValue);
        //                            }
        //                        }
        //#endif
        //                    }

        //                    // TODO: 释放未托管的资源(未托管的对象)并重写终结器
        //                    // TODO: 将大型字段设置为 null
        //                    disposedValue = true;
        //                }
        //                base.Dispose(disposing);
        //            }
        //        }
        //#endif
        //#endif
        //        #endregion
    }
}