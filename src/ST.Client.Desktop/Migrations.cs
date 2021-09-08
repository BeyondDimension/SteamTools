//using System.Application.Services;
//using System.Collections.Generic;
//using System.Diagnostics;
//using System.IO;
//using System.Linq;
//using System.Security;
//using System.Threading.Tasks;
//using System.Windows;
//using static System.Application.KeyConstants;

//namespace System.Application
//{
//    /// <summary>
//    /// 迁移
//    /// </summary>
//    public static class Migrations
//    {
//        /// <summary>
//        /// 获取正在运行的V1版本进程
//        /// </summary>
//        /// <returns></returns>
//        public static IEnumerable<Process> GetRunningV1Processes()
//        {
//            var query = from p in Process.GetProcessesByName("Steam++")
//                        where p.ProcessName == "Steam++"
//                        let filePath = p.MainModule?.FileName
//                        where !string.IsNullOrWhiteSpace(filePath)
//                        let fileVersionInfo = FileVersionInfo.GetVersionInfo(filePath).FileVersion.TryParseVersion()
//                        where fileVersionInfo != null && fileVersionInfo.Major == 1
//                        select p;
//            return query;
//        }

//        /// <summary>
//        /// 当前正在运行V1版本
//        /// </summary>
//        public static bool IsRunningV1 => GetRunningV1Processes().Any();

//        /// <summary>
//        /// 检测是否需要从 V1 版本迁移
//        /// </summary>
//        /// <returns></returns>
//        public static (bool needMigrate, Action delete) CheckV1()
//        {
//            var paths = new HashSet<string>
//                        {
//                            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
//                            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
//                        }.AsEnumerable();
//            paths = paths.Select(x => Path.Combine(x, "rmbgame.net"));
//            var needMigrate = paths.Any(x => Directory.Exists(x));
//            void Delete()
//            {
//                var processes = GetRunningV1Processes();
//                if (processes.Any())
//                {
//                    foreach (var p in processes)
//                    {
//                        p.Kill();
//                    }
//                    IHostsFileService.Instance.RemoveHostsByTag();
//                }
//                foreach (var path in paths)
//                {
//                    if (Directory.Exists(path))
//                    {
//                        Directory.Delete(path, true);
//                    }
//                }
//            }
//            return (needMigrate, Delete);
//        }

//        /// <summary>
//        /// 删除 V1 版本的配置
//        /// </summary>
//        public static void DeleteV1()
//        {
//            (var needMigrate, var delete) = CheckV1();
//            if (needMigrate) delete();
//        }

//        /// <summary>
//        /// 启动时检测从 V1 版本 迁移配置文件等信息
//        /// </summary>
//        public static void OnStartFromV1()
//        {
//            return; // 当 2.0 Ready 时取消此行
//            Task.Run(async () =>
//            {
//                try
//                {
//                    var s = IStorage.Instance;
//                    var examined = await s.GetAsync<bool>(EXAMINED_MIGRATE_FROM_V1);
//                    if (!examined)
//                    {
//                        (var needMigrate, var delete) = CheckV1();
//                        if (needMigrate)
//                        {
//                            var r = await MessageBoxCompat.ShowAsync(
//                                "检测到 V1 版本配置文件，" +
//                                "如果包含令牌数据，请先手动迁移令牌，" +
//                                "如果不存在令牌数据则可直接删除，是否直接删除？",
//                                "警告",
//                                MessageBoxButtonCompat.OKCancel,
//                                MessageBoxImageCompat.Warning);
//                            if (r == MessageBoxResultCompat.OK)
//                            {
//                                delete();

//                                // 执行迁移成功后，在AppData下写入文件或写入键值对中的一个标记
//                                // 下次启动检查标记，跳过迁移
//                                await s.SetAsync(EXAMINED_MIGRATE_FROM_V1, true);
//                            }
//                        }
//                    }
//                }
//                catch (Exception e)
//                {
//                    Log.Error(nameof(Migrations), e, nameof(OnStartFromV1));
//                }
//            });
//        }
//    }
//}