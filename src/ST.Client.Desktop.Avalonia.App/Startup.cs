namespace System.Application.UI
{
    internal static partial class Startup
    {
        static bool isInitialized;

        public static void Init(bool isMainProcess)
        {
            if (!isInitialized)
            {
                isInitialized = true;
                FileSystemDesktop.InitFileSystem();
                if (isMainProcess)
                {
                    ModelValidatorProvider.Init();
                }
                InitDI(isMainProcess);
            }
        }
    }
}