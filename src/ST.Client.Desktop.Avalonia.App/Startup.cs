namespace System.Application.UI
{
    internal static partial class Startup
    {
        static bool isInitialized;

        public static void Init(CommandLineTools.DILevel level)
        {
            if (!isInitialized)
            {
                isInitialized = true;
                FileSystemDesktop.InitFileSystem();
                if (level.HasFlag(CommandLineTools.DILevel.ModelValidator))
                {
                    ModelValidatorProvider.Init();
                }
                InitDI(level);
            }
        }
    }
}