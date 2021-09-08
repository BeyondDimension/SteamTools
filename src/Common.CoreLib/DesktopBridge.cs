namespace System
{
    public abstract class DesktopBridge
    {
        protected DesktopBridge() => throw new NotSupportedException();

        /// <summary>
        /// 指示当前应用程序是否正在通过 Desktop Bridge 运行在 UWP 上。
        /// </summary>
        public static bool IsRunningAsUwp { get; protected set; }
    }
}