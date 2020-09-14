using Ninject;
using SteamTool.Steam.Service;
using System;

namespace SteamTool.Steam.Service
{
    public class SteamService
    {
        private static SteamService instance = null;
        private static readonly object syncRoot = new Object();
        /// <summary>
        /// IoC 容器 
        /// </summary>
        private IKernel Kernel { get; set; } = new StandardKernel();


        public SteamService()
        {
            Kernel.Bind<SteamDLLService>().To<SteamDLLService>();
        }

        public TInterface Get<TInterface>()
        {
            return Kernel.Get<TInterface>();
        }

        public void Dispose()
        {
            Kernel.Dispose();
        }

        /// <summary>
        /// 创建或者从缓存中获取对应业务类的实例
        /// </summary>
        public static SteamService Instance
        {
            get
            {
                if (instance == null)
                {
                    lock (syncRoot)
                    {
                        if (instance == null)
                        {
                            instance = new SteamService();
                        }
                    }
                }
                return instance;
            }
        }
    }
}
