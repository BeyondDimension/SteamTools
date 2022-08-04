using System.Application.Services;

namespace System.Application.UI
{
    partial interface IApplication
    {
        IProgramHost ProgramHost { get; }

        public interface IStartupArgs
        {
            /// <summary>
            /// 当前是否是主进程
            /// </summary>
            bool IsMainProcess { get; }
        }

        public interface IDesktopStartupArgs : IStartupArgs
        {
            /// <summary>
            /// 当前是否是命令行工具进程
            /// </summary>
            bool IsCLTProcess { get; }

            /// <summary>
            /// 是否最小化启动
            /// </summary>
            bool IsMinimize { get; set; }

            /// <summary>
            /// 当前是否是用于托盘的独立进程
            /// </summary>
            bool IsTrayProcess { get; }

            /// <summary>
            /// 是否启动代理服务
            /// </summary>
            bool IsProxy { get; set; }

            EOnOff ProxyStatus { get; set; }
        }

        public interface IProgramHost : IStartupArgs
        {
            void ConfigureServices(DILevel level, bool isTrace = false);

            void OnStartup();

            void InitVisualStudioAppCenterSDK();

            IApplication Application { get; }
        }

        public interface IDesktopProgramHost : IProgramHost, IDesktopStartupArgs
        {
            void OnCreateAppExecuted(Action<IViewModelManager>? handlerViewModelManager = null, bool isTrace = false);

            DeploymentMode DeploymentMode { get; }
        }
    }
}
