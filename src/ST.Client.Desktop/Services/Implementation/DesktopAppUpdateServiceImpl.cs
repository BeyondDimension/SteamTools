using Microsoft.Extensions.Options;
using System.Application.Models;
using System.Application.Properties;
using System.Application.UI;
using System.Diagnostics;
using System.IO;
using System.Properties;
using System.Text;
using static System.Application.Services.IAppUpdateService;

namespace System.Application.Services.Implementation
{
    public abstract class DesktopAppUpdateServiceImpl : AppUpdateServiceImpl
    {
        public DesktopAppUpdateServiceImpl(IToast toast, ICloudServiceClient client, IOptions<AppSettings> options) : base(toast, client, options)
        {
        }

        protected override Version OSVersion => Environment.OSVersion.Version;

        /// <summary>
        /// 当更新下载完毕并校验完成时，即将退出程序之前
        /// </summary>
        protected abstract void OnExit();

        protected override void OverwriteUpgrade(string value, bool isIncrement)
        {
            if (isIncrement) // 增量更新
            {
                OverwriteUpgradePrivate(value);
            }
            else // 全量更新
            {
                var dirPath = Path.Combine(IOPath.BaseDirectory, Path.GetFileNameWithoutExtension(value));

                if (Directory.Exists(dirPath))
                {
                    Directory.Delete(dirPath, true);
                }

                if (TarGZipHelper.Unpack(value, dirPath, progress: new Progress<float>(OnReportDecompressing), maxProgress: MaxProgressValue))
                {
                    OnReport(MaxProgressValue);
                    IOPath.FileTryDelete(value);
                    OverwriteUpgradePrivate(dirPath);
                }
                else
                {
                    toast.Show(SR.UpdateUnpackFail);
                    OnReport(MaxProgressValue);
                }
            }

            void OverwriteUpgradePrivate(string dirPath)
            {
                OnExit();

                var updateCommandPath = Path.Combine(IOPath.CacheDirectory, "update.cmd");
                IOPath.FileIfExistsItDelete(updateCommandPath);

                var updateCommand = string.Format(
                   SR.ProgramUpdateCmd_,
                   AppHelper.ProgramName,
                   dirPath.TrimEnd(Path.DirectorySeparatorChar),
                   IOPath.BaseDirectory,
                   AppHelper.ProgramPath);

                updateCommand = "chcp" + Environment.NewLine + "chcp 65001" + Environment.NewLine + updateCommand;

                File.WriteAllText(updateCommandPath, updateCommand, Encoding.UTF8);

                using var p = new Process();
                p.StartInfo.FileName = updateCommandPath;
                p.StartInfo.UseShellExecute = false;
                p.StartInfo.CreateNoWindow = !ThisAssembly.Debuggable; // 不显示程序窗口
                p.StartInfo.Verb = "runas"; // 管理员权限运行
                p.Start(); // 启动程序
            }
        }
    }
}