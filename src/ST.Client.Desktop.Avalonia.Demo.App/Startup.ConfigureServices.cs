using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using System.Application.Models;
using System.Application.Services;
using System.Application.Services.CloudService.Clients.Abstractions;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace System.Application.UI
{
    partial class Startup
    {
        static void InitDI(bool _)
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

        sealed class MockCloudServiceClient : ICloudServiceClient, IAccountClient, IManageClient, IAuthMessageClient, IVersionClient, IActiveUserClient
        {
            public IAccountClient Account => this;

            public IManageClient Manage => this;

            public IAuthMessageClient AuthMessage => this;

            public IVersionClient Version => this;

            public IActiveUserClient ActiveUser => this;

            public Task<IApiResponse<string>> ChangeBindPhoneNumber(ChangePhoneNumberRequest.Validation request)
            {
                return Task.FromResult(ApiResponse.Ok("123"));
            }

            public Task<IApiResponse> ChangeBindPhoneNumber(ChangePhoneNumberRequest.New request)
            {
                return Task.FromResult(ApiResponse.Ok());
            }

            public Task<IApiResponse<AppVersionDTO?>> CheckUpdate(Guid id, Platform platform, DeviceIdiom deviceIdiom, ArchitectureFlags supportedAbis, Version osVersion)
            {
                return Task.FromResult(ApiResponse.Ok<AppVersionDTO?>(default));
            }

            public Task<IApiResponse<ClockInResponse>> ClockIn()
            {
                return Task.FromResult(ApiResponse.Ok(new ClockInResponse
                {
                    Level = 99,
                }));
            }

            public Task<IApiResponse> DeleteAccount()
            {
                return Task.FromResult(ApiResponse.Ok());
            }

            public Task<IApiResponse> Download(bool isAnonymous, string requestUri, string cacheFilePath, IProgress<float> progress, CancellationToken cancellationToken = default)
            {
                return Task.FromResult(ApiResponse.Ok());
            }

            public Task<HttpResponseMessage> Forward(HttpRequestMessage request, HttpCompletionOption completionOption, CancellationToken cancellationToken = default)
            {
                throw new NotImplementedException();
            }

            public Task<IApiResponse<LoginOrRegisterResponse>> LoginOrRegister(LoginOrRegisterRequest request)
            {
                return Task.FromResult(ApiResponse.Ok(new LoginOrRegisterResponse
                {
                    AuthToken = new JWTEntity
                    {
                        AccessToken = "123",
                        ExpiresIn = DateTimeOffset.MaxValue,
                        RefreshToken = "321",
                    },
                    IsLoginOrRegister = true,
                    User = new UserInfoDTO
                    {
                        Level = 98,
                        NickName = "User",
                    },
                }));
            }

            public Task<IApiResponse> Post(ActiveUserRecordDTO record)
            {
                return Task.FromResult(ApiResponse.Ok());
            }

            public Task<IApiResponse<JWTEntity>> RefreshToken(string refresh_token)
            {
                throw new NotImplementedException();
            }

            public ValueTask<IApiResponse> SendSms(SendSmsRequest request)
            {
                return ValueTask.FromResult(ApiResponse.Ok());
            }
        }
    }
}