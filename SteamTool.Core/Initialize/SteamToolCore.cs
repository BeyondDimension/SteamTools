using Ninject;
using SteamTool.Core;
using System;
using System.Collections.Generic;
using System.Text;

namespace SteamTool.Core
{
    public class SteamToolCore
    {
        private static SteamToolCore instance = null;
        private static readonly object syncRoot = new Object();
        /// <summary>
        /// IoC 容器 
        /// </summary>
        private IKernel Kernel { get; set; } = new StandardKernel();


        public SteamToolCore()
        {
            Kernel.Bind<ConfigService>().ToConstant(new ConfigService());
            Kernel.Bind<RegistryKeyService>().To<RegistryKeyService>();
            Kernel.Bind<SteamToolService>().ToSelf();
            Kernel.Bind<VdfService>().To<VdfService>();
            Kernel.Bind<HttpServices>().ToConstant(new HttpServices());
            Kernel.Bind<HostsService>().ToConstant(new HostsService());
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
        public static SteamToolCore Instance
        {
            get
            {
                if (instance == null)
                {
                    lock (syncRoot)
                    {
                        if (instance == null)
                        {
                            instance = new SteamToolCore();
                        }
                    }
                }
                return instance;
            }
        }
    }
}
