// ReSharper disable once CheckNamespace
namespace BD.WTTS;

partial interface IApplication
{
    IProgramHost ProgramHost { get; }

    interface IStartupArgs
    {
        /// <summary>
        /// 当前是否是主进程
        /// </summary>
        bool IsMainProcess { get; }
    }

    interface IDesktopStartupArgs : IStartupArgs
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

        OnOffToggle ProxyStatus { get; set; }
    }

    interface IProgramHost : IStartupArgs
    {
        void ConfigureServices(AppServicesLevel level, bool isTrace = false);

        void OnStartup();

        void InitVisualStudioAppCenterSDK();

        IApplication Application { get; }
    }

    interface IDesktopProgramHost : IProgramHost, IDesktopStartupArgs
    {
        void OnCreateAppExecuted(Action<IViewModelManager>? handlerViewModelManager = null, bool isTrace = false);

        DeploymentMode DeploymentMode { get; }
    }
}