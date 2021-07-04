#if !NOT_DI
using Microsoft.Extensions.DependencyInjection;
#endif
using System.Linq;
using System.Runtime.InteropServices;
#if !NOT_XE
using Xamarin.Essentials;
#endif

namespace System
{
    /// <summary>
    /// 依赖注入服务组(DependencyInjection)
    /// </summary>
    public static partial class DI
    {
#if !NOT_DI

        static IServiceProvider? value;

        internal static bool IsInit => value != null;

        /// <summary>
        /// 初始化依赖注入服务组(通过配置服务项的方式)
        /// </summary>
        /// <param name="configureServices"></param>
        public static void Init(Action<IServiceCollection> configureServices)
        {
            var services = new ServiceCollection();
            configureServices(services);
            Init(services.BuildServiceProvider());
        }

        /// <summary>
        /// 初始化依赖注入服务组(直接赋值)
        /// </summary>
        /// <param name="serviceProvider"></param>
        public static void Init(IServiceProvider serviceProvider)
        {
            value = serviceProvider;
        }

#endif

        /// <inheritdoc cref="System.Platform"/>
        public static Platform Platform { get; }

        /// <inheritdoc cref="System.DeviceIdiom"/>
        public static DeviceIdiom DeviceIdiom { get; private set; }

        /// <summary>
        /// 当前是否使用Mono运行时
        /// </summary>
        public static bool IsRunningOnMono { get; }

        /// <summary>
        /// 当前进程运行的构架为 <see cref="Architecture.X86"/> 或 <see cref="Architecture.X64"/>
        /// </summary>
        public static bool IsX86OrX64 { get; }

        /// <summary>
        /// 当前进程运行的构架为 <see cref="Architecture.Arm64"/>
        /// </summary>
        public static bool IsArm64 { get; }

        /// <summary>
        /// 当前是否运行在 macOS 上
        /// </summary>
        public static bool IsmacOS { get; }

        /// <summary>
        /// 当前是否运行在 iOS/iPadOS/watchOS 上
        /// </summary>
        public static bool IsiOSOriPadOSOrwatchOS { get; }

        /// <summary>
        /// 当前是否运行在 >= Windows 10 上
        /// </summary>
        public static bool IsWindows10OrLater { get; }

        static bool? _IsDesktopBridge;
        public static bool IsDesktopBridge
        {
            get => _IsDesktopBridge ?? false;
            set
            {
                if (_IsDesktopBridge.HasValue) throw new NotSupportedException();
                _IsDesktopBridge = value;
            }
        }

        const string DesktopWindowTypeNames =
            "Avalonia.Controls.Window, Avalonia.Controls" +
            "\n" +
            "System.Windows.Forms.Form, System.Windows.Forms" +
            "\n" +
            "System.Windows.Window, PresentationFramework";

        static DI()
        {
            IsRunningOnMono = Type.GetType("Mono.Runtime") != null;
            var processArchitecture = RuntimeInformation.ProcessArchitecture;
            IsX86OrX64 = processArchitecture is Architecture.X64 or Architecture.X86;
            IsArm64 = processArchitecture == Architecture.Arm64;
            static Platform RuntimeInformationOSPlatform()
            {
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                    return Platform.Windows;
                if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                    return Platform.Apple;
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                    return Platform.Linux;
                return Platform.Unknown;
            }
            static DeviceIdiom GetDeviceIdiom()
            {
                if (DesktopWindowTypeNames.Split('\n')
                    .Any(x => Type.GetType(x) != null))
                    return DeviceIdiom.Desktop;
                return DeviceIdiom.Unknown;
            }
#if !NOT_XE
            Platform = DeviceInfo.Platform.Convert();
            if (Platform == Platform.Unknown) Platform = RuntimeInformationOSPlatform();
            DeviceIdiom = DeviceInfo.Idiom.Convert();
            if (DeviceIdiom == DeviceIdiom.Unknown) DeviceIdiom = GetDeviceIdiom();
#else
            Platform = RuntimeInformationOSPlatform();
            DeviceIdiom = GetDeviceIdiom();
#endif
            if (Platform == Platform.Apple)
            {
                IsmacOS = DeviceIdiom == DeviceIdiom.Desktop;
                IsiOSOriPadOSOrwatchOS = !IsmacOS;
            }
            else if (Platform == Platform.Windows)
            {
                IsWindows10OrLater = Environment.OSVersion.Version.Major >= 10;
            }
        }

#if !NOT_DI

        static Exception GetDIGetFailException(Type serviceType) => new($"DI.Get* fail, serviceType: {serviceType}");

        /// <summary>
        /// 获取依赖注入服务
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static T Get<T>() where T : notnull
        {
            if (value == null)
            {
                throw GetDIGetFailException(typeof(T));
            }
            return value.GetRequiredService<T>();
        }

        /// <inheritdoc cref="Get{T}"/>
        public static T? Get_Nullable<T>() where T : notnull
        {
            if (value == null)
            {
                throw GetDIGetFailException(typeof(T));
            }
            return value.GetService<T>();
        }

        /// <inheritdoc cref="Get{T}"/>
        public static object Get(Type serviceType)
        {
            if (value == null)
            {
                throw GetDIGetFailException(serviceType);
            }
            return value.GetRequiredService(serviceType);
        }

        /// <inheritdoc cref="Get_Nullable{T}"/>
        public static object? Get_Nullable(Type serviceType)
        {
            if (value == null)
            {
                throw GetDIGetFailException(serviceType);
            }
            return value.GetService(serviceType);
        }

        public static IServiceScope CreateScope()
        {
            if (value == null)
            {
                throw new Exception("DI.CreateScope fail.");
            }
            return value.CreateScope();
        }

#endif
    }
}