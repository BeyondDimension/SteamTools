using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using System.Application.Services;
using System.Application.Services.CloudService;
using System.Collections.Generic;

namespace System.Application.UI
{
    partial class Startup
    {
        public static bool HasNotifyIcon => false;

        static void InitDI(CommandLineTools.DILevel _)
        {
            DI.Init(new MockServiceProvider(ConfigureServices));
        }

        static void ConfigureServices(IServiceCollection services)
        {
            services.AddLogging(cfg => cfg.AddProvider(NullLoggerProvider.Instance));

            services.AddSingleton<ICloudServiceClient, MockCloudServiceClient>();
        }

        sealed class MockServiceProvider : IServiceProvider
        {
            static readonly Type typeMock = typeof(Mock<>);
            readonly Dictionary<Type, object?> pairs = new();
            readonly IServiceProvider serviceProvider;

            public MockServiceProvider(Action<IServiceCollection> configureServices)
            {
                var services = new ServiceCollection();
                configureServices(services);
                serviceProvider = services.BuildServiceProvider();
            }

            public object? GetService(Type serviceType)
            {
                var service = serviceProvider.GetService(serviceType);
                if (service != null) return service;
                if (pairs.ContainsKey(serviceType)) return pairs[serviceType];
                var mockServiceType = typeMock.MakeGenericType(serviceType);
                var mockService = (Mock?)Activator.CreateInstance(mockServiceType);
                service = mockService?.Object;
                pairs.Add(serviceType, service);
                return service;
            }
        }
    }
}