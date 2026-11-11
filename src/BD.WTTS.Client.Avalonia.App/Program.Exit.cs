namespace BD.WTTS;

partial class Program
{
#if MACOS || LINUX
    static bool nativeUiInitialized;

    [System.Runtime.InteropServices.DllImport("/usr/lib/libSystem.B.dylib", EntryPoint = "_exit")]
    static extern void ExitMacOS(int exitCode);
#endif

    static int CompleteExit(int exitCode)
    {
#if MACOS || LINUX
        if (nativeUiInitialized)
        {
            System.Console.Out.Flush();
            System.Console.Error.Flush();
            ExitMacOS(exitCode);
        }
#endif
        return exitCode;
    }
}