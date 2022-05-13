namespace System.Diagnostics
{
    public static class Process2
    {
        public static bool OpenCoreByProcess(string url, Action<Exception>? onError = null)
        {
            try
            {
                var p = new ProcessStartInfo(url)
                {
                    UseShellExecute = true,
                };
                Process.Start(p);
                return true;
            }
            catch (Exception e)
            {
                // [Win32Exception: 找不到应用程序] 39次报告
                // 疑似缺失没有默认浏览器设置会导致此异常，可能与杀毒软件有关
                onError?.Invoke(e);
                return false;
            }
        }
    }
}
