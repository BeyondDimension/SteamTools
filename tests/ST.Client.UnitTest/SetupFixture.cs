using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NUnit.Framework;
using System.Application.Models;
using System.Application.Services;
using System.Application.Services.Implementation;
using System.IO;
using System.Logging;
using System.Net;
using System.Net.Http;

namespace System.Application
{
    [SetUpFixture]
    public class SetupFixture
    {
        public static bool DIInit { get; set; }

        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            // TODO: Add code here that is run before
            //  all tests in the assembly are run
            if (!DIInit)
            {
                ModelValidatorProvider.Init();
                DI.Init(ConfigureServices);

                var path = AppContext.BaseDirectory;
                var path1 = Path.Combine(path, "AppData");
                IOPath.DirCreateByNotExists(path1);
                var path2 = Path.Combine(path, "Cache");
                IOPath.DirCreateByNotExists(path2);
                string GetAppDataDirectory() => path1;
                string GetCacheDirectory() => path2;
                IOPath.InitFileSystem(GetAppDataDirectory, GetCacheDirectory);
            }
        }

        [OneTimeTearDown]
        public void OneTimeTearDown()
        {
            // TODO: Add code here that is run after
            //  all tests in the assembly have been run
#if !Desktop_UnitTest
            HostsFileTest.DeleteAllTempFileName();
#endif
        }

        const string DevAppVersion = "00000000000000000000000000000001";
        const string DevRSAPublicKey = "{\"v\":\"AQAB\",\"n\":\"u4iAALIX2NLyh9-GeWbBdNK2gpa1qFx1S2fFoRKuzxspTP4oyJ2uMF7xZ1yWogup-M7x3BPYrXMTzmyYTmJFDDsWt3nEl-mk6ABJ5RKeCUO1GfHXo8jvHod_pfs8gmFmyzSoYdxUp6BayXT7LkxZz9pO2ZEK2JU1dkSQKRyy_U9ceDsy9D-xsmkX2MrtYTdG51CxdFD_SrStI1YOEhbBv_A97JYJv_F5UyP2CGJ_zWG-MVWO-2Ir2AiVQMBR9PUfNweAGEAsm-AAnRHqIGHjOIMm8PqmkY4Lft2zwDE2TMcw-yUBOwYUvWanIKbO-T3KdBIc7ZbNe9Vut8aVWJa8UQ\"}";

        static void ConfigureServices(IServiceCollection services)
        {
            services.AddLogging(l => l.AddProvider(NUnitLoggerProvider.Instance));
            services.Configure<LoggerFilterOptions>(o =>
            {
                o.MinLevel = LogLevel.Trace;
            });

            var options = new AppSettings
            {
                ApiBaseUrl = "https://localhost:5001",
                AppVersion = Guid.ParseExact(DevAppVersion, "N"),
                RSASecret = DevRSAPublicKey,
            };
            // app 配置项
            services.TryAddOptions(options);

            // 添加安全服务
            services.AddSecurityService<EmbeddedAesDataProtectionProvider, EmptyLocalDataProtectionProvider>();

            // 模型验证框架
            services.TryAddModelValidator();

            services.AddRepositories();

            // 键值对存储
            services.TryAddStorage();

            // 业务平台用户管理
            services.TryAddUserManager();

            services.TryAddDesktopHttpPlatformHelper();

            // 服务端API调用
            services.TryAddCloudServiceClient<CloudServiceClient>(c =>
            {
#if NETCOREAPP3_0_OR_GREATER
                c.DefaultRequestVersion = HttpVersion.Version20;
#endif
#if NET5_0_OR_GREATER
                c.DefaultVersionPolicy = HttpVersionPolicy.RequestVersionExact;
#endif
            }, configureHandler:
#if NETCOREAPP2_1_OR_GREATER
            () => new SocketsHttpHandler
            {
                UseCookies = false,
                AutomaticDecompression = DecompressionMethods.GZip,
            }
#else
            null
#endif
            );

            // 通用 Http 服务
            services.AddHttpService();

            PlatformToastImpl.TryAddToast(services);
        }
    }

    internal sealed class PlatformToastImpl : ToastImpl
    {
        public PlatformToastImpl(IToastIntercept intercept) : base(intercept)
        {
        }

        protected override void PlatformShow(string text, int duration)
        {
            Log.Info("Toast", text);
        }

        public static IServiceCollection TryAddToast(IServiceCollection services)
          => TryAddToast<PlatformToastImpl>(services);
    }
}