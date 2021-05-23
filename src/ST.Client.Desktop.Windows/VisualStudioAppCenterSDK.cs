using Microsoft.AppCenter;
using Microsoft.AppCenter.Analytics;
using Microsoft.AppCenter.Crashes;
using System;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Security;
using System.Threading.Tasks;
using static System.Application.AppClientAttribute;
#if WINDOWS
using System.Reflection;
using static System.Application.VisualStudioAppCenterSDK;
using System.Text;
using Microsoft.AppCenter.Utils;
#if DEBUG
using static System.Application.UI.ViewModels.DebugPageViewModel;
#endif
#endif

namespace System.Application
{
    public static class VisualStudioAppCenterSDK
    {
        /// <summary>
        /// Visual Studio App Center
        /// <list type="bullet">
        /// <item>将移动开发人员常用的多种服务整合到一个集成的产品中。</item>
        /// <item>您可以构建，测试，分发和监控移动应用程序，还可以实施推送通知。</item>
        /// <item>https://docs.microsoft.com/zh-cn/appcenter/sdk/getting-started/xamarin</item>
        /// <item>https://visualstudio.microsoft.com/zh-hans/app-center</item>
        /// </list>
        /// </summary>
        public static void Init()
        {
            if (TryGetAppSecret(out var appSecret))
            {
#if WINDOWS
                var _applicationSettingsFactory = typeof(AppCenter).GetField("_applicationSettingsFactory", BindingFlags.NonPublic | BindingFlags.Static);
                _applicationSettingsFactory.ThrowIsNull(nameof(_applicationSettingsFactory)).SetValue(null, DI.Get<IApplicationSettingsFactory>());
#endif
                AppCenter.Start(appSecret, typeof(Analytics), typeof(Crashes));
            }
        }

        static readonly Lazy<string?> _AppSecret = new(() =>
        {
            var assembly = typeof(VisualStudioAppCenterSDK).Assembly;
            const string namespacePrefix = "System.Application.Resources.";
            Stream? func(string x) => assembly.GetManifestResourceStream(x);
            var r = GetResValue(func,
#if XAMARIN_MAC || MONO_MAC || MAC
                    "appcenter-secret-mac",
#elif __ANDROID__
                    "appcenter-secret-android",
#elif __IOS__
                    "appcenter-secret-ios",
#else
                    "appcenter-secret",
#endif
                    isSingle: false,
                namespacePrefix,
                ResValueFormat.StringGuidD);
            return r;
        });
        static readonly Lazy<bool> _HasAppSecret = new(() => !string.IsNullOrWhiteSpace(_AppSecret.Value));

        internal static bool TryGetAppSecret([NotNullWhen(true)] out string? appSecret)
        {
            appSecret = _AppSecret.Value;
            return _HasAppSecret.Value;
        }
    }
}

#if WINDOWS
namespace Microsoft.AppCenter.Utils
{
    internal sealed class ApplicationSettings : IApplicationSettings
    {
        static readonly object configLock = new();
        readonly IStorage storage;

        public ApplicationSettings(IStorage storage)
        {
            this.storage = storage;
        }

        public bool ContainsKey(string key)
        {
            lock (configLock)
            {
                Func<Task<bool>> func = () => storage.ContainsKeyAsync(key);
                var r = func.RunSync();
                return r;
            }
        }

        public T? GetValue<T>(string key, T? defaultValue = default) where T : notnull
        {
            lock (configLock)
            {
                Func<Task<string?>> func = () => storage.GetAsync(key);
                var r = func.RunSync();
                if (r != null)
                {
                    try
                    {
                        var r2 = (T)TypeDescriptor.GetConverter(typeof(T)).ConvertFromInvariantString(r);
                        return r2;
                    }
                    catch
                    {
                    }
                }
            }
            return defaultValue;
        }

        public void Remove(string key)
        {
            lock (configLock)
            {
                Func<Task> func = () => storage.RemoveAsync(key);
                func.RunSync();
            }
        }

        public void SetValue(string key, object value)
        {
            var invariant = value != null ? TypeDescriptor.GetConverter(value.GetType()).ConvertToInvariantString(value) : null;
            lock (configLock)
            {
                Func<Task> func = () => storage.SetAsync(key, invariant);
                func.RunSync();
            }
        }
    }

    internal sealed class ApplicationSettingsFactory : IApplicationSettingsFactory
    {
        public IApplicationSettings CreateApplicationSettings()
        {
            var r = DI.Get<IApplicationSettings>();
            return r;
        }
    }

#if DEBUG
    internal sealed class TestAppCenter : ITestAppCenter
    {
        readonly IApplicationSettings settings;
        public TestAppCenter(IApplicationSettings settings)
        {
            this.settings = settings;
        }

        public void Test(StringBuilder @string)
        {
            var newGuid = Guid.NewGuid();
            settings.SetValue("IDTest", newGuid);
            var hasIDTest = settings.ContainsKey("IDTest");
            @string.AppendLine($"TestAppCenter.HasIDTest: {hasIDTest}");
            var newGuid2 = settings.GetValue<Guid>("IDTest");
            @string.AppendLine($"TestAppCenter.IDTest: {newGuid == newGuid2}");
        }
    }
#endif
}

namespace Microsoft.Extensions.DependencyInjection
{
    public static partial class ServiceCollectionExtensions
    {
        static IServiceCollection AddMSAppCenterApplicationSettings(this IServiceCollection services)
        {
            if (TryGetAppSecret(out var _))
            {
                services.AddSingleton<IApplicationSettings, ApplicationSettings>();
                services.AddSingleton<IApplicationSettingsFactory, ApplicationSettingsFactory>();
#if DEBUG
                services.AddSingleton<ITestAppCenter, TestAppCenter>();
#endif
            }
            return services;
        }
    }
}
#endif