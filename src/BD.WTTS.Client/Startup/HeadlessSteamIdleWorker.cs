#if (WINDOWS || MACCATALYST || MACOS || LINUX) && !(IOS || ANDROID)
using System.Runtime.InteropServices;
using System.Threading;

namespace BD.WTTS;

/// <summary>A native Steam session with no application host, plugins or game-list coordinator.</summary>
internal static class HeadlessSteamIdleWorker
{
    internal static int Run(int appId)
    {
        try
        {
            if (appId <= 0)
                throw new ArgumentOutOfRangeException(nameof(appId));

            using var exclusion = new Mutex(false, $"SteamTools.Idle.App.{appId}",
                new NamedWaitHandleOptions { CurrentUserOnly = true, CurrentSessionOnly = false });
            try
            {
                if (!exclusion.WaitOne(0))
                {
                    Console.Error.WriteLine($"A Steam idle worker is already running for AppID {appId}.");
                    return 1;
                }
            }
            catch (AbandonedMutexException)
            {
                // An abruptly terminated worker has no live native session to preserve.
            }

            try
            {
                return RunSession(appId);
            }
            finally
            {
                // Keep acquisition, session lifetime and release on the same thread.
                exclusion.ReleaseMutex();
            }
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine(ex);
            return 1;
        }
    }

    static int RunSession(int appId)
    {
        if (!SupportsProcessArchitecture(RuntimeInformation.ProcessArchitecture, OperatingSystem.IsMacOS()))
            throw new PlatformNotSupportedException("The Steam local API supports x86 and x64, and arm64 on macOS.");

        var libraryDirectory = BD.SteamClient.Services.Implementation.SteamServiceImpl.ResolveSteamDynamicLinkLibraryPath(
            ReadConfiguredSteamProgramPath());
        if (string.IsNullOrWhiteSpace(libraryDirectory))
            throw new DirectoryNotFoundException("The Steam client library directory could not be found.");

        SAM.API.Steam.GetInstallPathDelegate = () => libraryDirectory;
        try
        {
            using var shutdown = new ManualResetEventSlim();
            using var completed = new ManualResetEventSlim();
            var workerThread = Environment.CurrentManagedThreadId;
            var shutdownGate = new object();
            var disposed = false;
            EventHandler processExit = (_, _) =>
            {
                lock (shutdownGate)
                {
                    if (disposed) return;
                    shutdown.Set();
                    if (Environment.CurrentManagedThreadId != workerThread)
                        completed.Wait(TimeSpan.FromSeconds(5));
                }
            };
            AppDomain.CurrentDomain.ProcessExit += processExit;
            ConsoleCancelEventHandler cancel = (_, e) =>
            {
                e.Cancel = true;
                lock (shutdownGate)
                {
                    if (!disposed) shutdown.Set();
                }
            };
            Console.CancelKeyPress += cancel;
            void RequestSignalShutdown(PosixSignalContext context)
            {
                context.Cancel = true;
                lock (shutdownGate)
                {
                    if (disposed) return;
                    shutdown.Set();
#if WINDOWS
                    // Windows termination signals cannot be canceled. Give the owner
                    // time to release the native pipe before this handler returns.
                    if (Environment.CurrentManagedThreadId != workerThread)
                        completed.Wait(TimeSpan.FromSeconds(5));
#endif
                }
            }
            try
            {
                using var terminate = PosixSignalRegistration.Create(PosixSignal.SIGTERM, RequestSignalShutdown);
                using var hangup = PosixSignalRegistration.Create(PosixSignal.SIGHUP, RequestSignalShutdown);
                try
                {
                    using var client = new SAM.API.Client();
                    if (!shutdown.IsSet)
                    {
                        if (!client.Initialize(appId))
                            throw new InvalidOperationException($"Failed to initialize Steam AppID {appId}.");
                        // This process owns the native pipe until a graceful stop is requested.
                        shutdown.Wait();
                    }
                }
                finally
                {
                    completed.Set();
                }
            }
            finally
            {
                Console.CancelKeyPress -= cancel;
                completed.Set();
                lock (shutdownGate)
                {
                    disposed = true;
                    AppDomain.CurrentDomain.ProcessExit -= processExit;
                }
            }
        }
        finally
        {
            SAM.API.Steam.GetInstallPathDelegate = null;
        }
        return 0;
    }

    internal static bool SupportsProcessArchitecture(Architecture architecture, bool isMacOS)
        => architecture is Architecture.X86 or Architecture.X64 || (isMacOS && architecture == Architecture.Arm64);

    static string? ReadConfiguredSteamProgramPath()
    {
        // Resolve the same platform directories without migrations, settings monitors or writes.
#if WINDOWS
        var packaged = DesktopBridgeHelper.Init();
        var appDataDirectory = packaged ? WindowsRuntimeFileSystem.AppDataDirectory
            : WindowsPlatformServiceImpl.CurrentAppIsInstallVersion ? WindowsFileSystem.AppDataDirectory
            : FileSystem2.BaseDirectory.AppDataDirectory;
#elif LINUX
        var appDataDirectory = LinuxFileSystem.AppDataDirectory;
#elif MACOS || MACCATALYST
        var appDataDirectory = MacCatalystFileSystem.AppDataDirectory;
#endif
        var relativePath = Path.Combine("Settings", nameof(SteamSettings) + ".json");
        var path = Path.Combine(appDataDirectory, relativePath);
        if (!File.Exists(path))
        {
#if WINDOWS || LINUX
#if WINDOWS
            if (packaged) return null;
#endif
            // An existing portable configuration may not yet have been migrated by the GUI.
            path = Path.Combine(FileSystem2.BaseDirectory.AppDataDirectory, relativePath);
            if (!File.Exists(path)) return null;
#else
            return null;
#endif
        }
        using var stream = File.Open(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite | FileShare.Delete);
        using var document = JsonDocument.Parse(stream, new JsonDocumentOptions
        {
            AllowTrailingCommas = true,
            CommentHandling = JsonCommentHandling.Skip,
        });
        if (!document.RootElement.TryGetProperty(nameof(SteamSettings), out var settings) ||
            !settings.TryGetProperty(nameof(SteamSettings_.SteamProgramPath), out var programPath))
            return null;
        return programPath.GetString();
    }
}
#endif
