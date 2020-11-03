using System;
using Ninject;
using SteamTool.WebApi.Service;
using SteamTool.WebApi.Service.SteamDb;

namespace SteamTool.Service
{
    public class WebApiService
    {
            private static WebApiService instance = null;
            private static readonly object syncRoot = new Object();
            /// <summary>
            /// IoC 容器 
            /// </summary>
            private IKernel Kernel { get; set; } = new StandardKernel();


            public WebApiService()
            {
                Kernel.Bind<SteamDbApiService>().To<SteamDbApiService>();
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
            public static WebApiService Instance
            {
                get
                {
                    if (instance == null)
                    {
                        lock (syncRoot)
                        {
                            if (instance == null)
                            {
                                instance = new WebApiService();
                            }
                        }
                    }
                    return instance;
                }
            }
    }
}
