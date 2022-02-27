using System.Application.Services;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace System.Application
{
    /// <summary>
    /// 新版本破坏性更改迁移
    /// </summary>
    public static class Migrations
    {
        static Version? PreviousVersion { get; } = Version.TryParse(VersionTracking2.PreviousVersion, out var value) ? value : null;

        /// <summary>
        /// 开始迁移(在 DI 初始化完毕后调用)
        /// </summary>
        public static void Up()
        {
            // 可以删除 /AppData/application2.dbf 表 0984415E 使此 if 返回 True
            // 目前此数据库中仅有这一张表，所以可以直接删除文件，但之后可能会新增新表
            if (VersionTracking2.IsFirstLaunchForCurrentVersion) // 当前版本号第一次启动
            {
                if (PreviousVersion != null) // 上一次运行的版本小于 2.6.2 时将执行以下迁移
                {
                    if (PreviousVersion < new Version(2, 6, 2))
                    {
                        var cacheScript = Path.Combine(IOPath.CacheDirectory, IScriptManager.DirName);

                        if (Directory.Exists(cacheScript))
                        {
                            try
                            {
                                Directory.Delete(cacheScript, true);
                            }
                            catch (Exception e)
                            {
                                Log.Error("RemoveJSCache", e, "RemoveFileError");
                            }
                        }
                    }
                }
            }
        }
    }
}
