// ReSharper disable once CheckNamespace
namespace BD.WTTS;

partial class Startup // 调试控制台
{
#if DEBUG
    /// <summary>
    /// 调试控制台
    /// </summary>
    protected static class DebugConsole
    {
        public static readonly bool WriteAssemblyLoad = false;
        public static readonly bool WriteAssemblyResolve = false;

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void WriteLine(FormattableString str)
        {
            Console.WriteLine(str);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void Write(string str)
        {
            Console.Write(str);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void WriteLine(string str)
        {
            Console.WriteLine(str);
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void WriteInfo()
        {
            WriteLine($"CurrentUICulture: {CultureInfo.CurrentUICulture}");
#if WINDOWS
            //#if (WINDOWS || MACCATALYST || MACOS || LINUX) && !(IOS || ANDROID)
            WriteLine($"RID: {GlobalDllImportResolver.RID}");
#endif
            WriteLine($"DefaultFontFamilyName: {SkiaSharp.SKTypeface.Default.FamilyName}");
            WriteLine($"DefaultFontFamilyName2: {IPlatformService.Instance.GetDefaultFontFamily()}");
            WriteLine("FontFamilies: ");
            foreach (var item in SkiaSharp.SKFontManager.Default.GetFontFamilies())
            {
                WriteLine(item);
            }
#if DEBUG
            Write($"OS: ");
#if WINDOWS
            WriteLine("WINDOWS");
#elif MACCATALYST
            WriteLine("MACCATALYST");
#elif MACOS
            WriteLine("MACOS");
#elif LINUX
            WriteLine("LINUX");
#elif IOS
            WriteLine("IOS");
#elif ANDROID
            WriteLine("ANDROID");
#else
            WriteLine();
#endif
            WriteLine($"Avalonia.Version: {IApplication.Instance.GetType().Assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion}");
            WriteLine($"Environment.Version: {Environment.Version}");
            WriteLine($"args: {string.Join(' ', Environment.GetCommandLineArgs())}");
            WriteLine($"AppContext.BaseDirectory: {AppContext.BaseDirectory}");
            //WriteLine($"AppDomain.CurrentDomain.BaseDirectory: {AppDomain.CurrentDomain.BaseDirectory}");
            WriteLine($"Environment.CurrentDirectory: {Environment.CurrentDirectory}");
            WriteLine($"Environment.ProcessPath: {Environment.ProcessPath}");
            WriteLine($"Environment.UserInteractive: {Environment.UserInteractive}");
            WriteLine($"Assembly.GetEntryAssembly()?.Location: {Assembly.GetEntryAssembly()?.Location}");
            WriteLine($"Directory.GetCurrentDirectory: {Directory.GetCurrentDirectory()}");
            WriteLine($"CurrentThread.ManagedThreadId: {Environment.CurrentManagedThreadId}");
            WriteLine($"CurrentThread.ApartmentState: {Thread.CurrentThread.GetApartmentState()}");
#endif
        }
    }
#endif
}