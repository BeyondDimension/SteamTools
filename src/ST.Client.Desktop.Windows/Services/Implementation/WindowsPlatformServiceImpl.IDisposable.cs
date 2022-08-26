namespace System.Application.Services.Implementation
{
    partial class WindowsPlatformServiceImpl : IDisposable
    {
        /// <summary>
        /// 用于 <see cref="IOPath.GetCacheFilePath(string, string, string)"/> 中 dirName，临时文件夹名称，在程序退出时将删除整个文件夹
        /// </summary>
        const string CacheTempDirName = "Temporary";

        bool disposedValue;

        private void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: 释放托管状态(托管对象)
                    isLightOrDarkThemeWatch?.Dispose();

                    IOPath.TryDeleteCacheSubDir(CacheTempDirName);
                }

                // TODO: 释放未托管的资源(未托管的对象)并重写终结器
                // TODO: 将大型字段设置为 null
                disposedValue = true;
            }
        }

        // // TODO: 仅当“Dispose(bool disposing)”拥有用于释放未托管资源的代码时才替代终结器
        // ~WindowsDesktopPlatformServiceImpl()
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
}