using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Application.Models;
using System.Application.Services;
using System.Application.Services.Implementation;
using System.Logging;
using System.Net;
#if MONOANDROID || __ANDROID__
#if MAUI
using EssentialsFileSystem = Microsoft.Maui.Storage.FileSystem;
#else
using EssentialsFileSystem = Xamarin.Essentials.FileSystem;
#endif
#endif

namespace System.Application;

partial class SetupFixture
{
    public static bool DIInit { get; set; }

    sealed class FileSystemTest : IOPath.FileSystemBase
    {
        private FileSystemTest() => throw new NotSupportedException();

        /// <summary>
        /// 初始化文件系统
        /// </summary>
        public static void InitFileSystem()
        {
#if MONOANDROID || __ANDROID__
            InitFileSystem(GetAppDataDirectory, GetCacheDirectory);
            string GetAppDataDirectory() => EssentialsFileSystem.AppDataDirectory;
            string GetCacheDirectory() => EssentialsFileSystem.CacheDirectory;
#else
            var path = AppContext.BaseDirectory;
            var path1 = Path.Combine(path, "AppData");
            IOPath.DirCreateByNotExists(path1);
            var path2 = Path.Combine(path, "Cache");
            IOPath.DirCreateByNotExists(path2);
            string GetAppDataDirectory() => path1;
            string GetCacheDirectory() => path2;
            InitFileSystem(GetAppDataDirectory, GetCacheDirectory);
#endif
        }
    }

    public static void OneTimeSetUpCore(Action<IServiceCollection> configureServices)
    {
        if (!DIInit)
        {
            ModelValidatorProvider.Init();
            DI.ConfigureServices(configureServices);
            FileSystemTest.InitFileSystem();
        }
    }

    public static void OneTimeTearDownCore()
    {
#if !Desktop_UnitTest && !__ANDROID__
        HostsFileTest.DeleteAllTempFileName();
#endif
    }

    //const string DevAppVersion = "00000000000000000000000000000001";
    const string DevRSAPublicKey = "{\"v\":\"AQAB\",\"n\":\"u4iAALIX2NLyh9-GeWbBdNK2gpa1qFx1S2fFoRKuzxspTP4oyJ2uMF7xZ1yWogup-M7x3BPYrXMTzmyYTmJFDDsWt3nEl-mk6ABJ5RKeCUO1GfHXo8jvHod_pfs8gmFmyzSoYdxUp6BayXT7LkxZz9pO2ZEK2JU1dkSQKRyy_U9ceDsy9D-xsmkX2MrtYTdG51CxdFD_SrStI1YOEhbBv_A97JYJv_F5UyP2CGJ_zWG-MVWO-2Ir2AiVQMBR9PUfNweAGEAsm-AAnRHqIGHjOIMm8PqmkY4Lft2zwDE2TMcw-yUBOwYUvWanIKbO-T3KdBIc7ZbNe9Vut8aVWJa8UQ\"}";

    public static void ConfigureServices(IServiceCollection services)
    {
        services.AddLogging(l => l.AddProvider(NUnitLoggerProvider.Instance));
        services.Configure<LoggerFilterOptions>(o =>
        {
            o.MinLevel = LogLevel.Trace;
        });

#if ANDROID || IOS || __ANDROID__ || MAUI
        services.TryAddEssentials();
#endif

        var options = new AppSettings
        {
            ApiBaseUrl = "https://localhost:5001",
            //AppVersion = Guid.ParseExact(DevAppVersion, "N"),
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
#if ANDROID || IOS || __ANDROID__ || MAUI
        services.TryAddEssentialsSecureStorage();
#else
        services.TryAddRepositorySecureStorage();
#endif

        // 业务平台用户管理
        services.TryAddUserManager();

        services.TryAddClientHttpPlatformHelperService();

        // 服务端API调用
        services.TryAddCloudServiceClient<CloudServiceClient>(c =>
        {
#if NETCOREAPP3_0_OR_GREATER
            c.DefaultRequestVersion = HttpVersion.Version20;
#endif
#if NET5_0_OR_GREATER
            c.DefaultVersionPolicy = HttpVersionPolicy.RequestVersionOrHigher;
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

internal sealed class PlatformToastImpl : ToastBaseImpl
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
