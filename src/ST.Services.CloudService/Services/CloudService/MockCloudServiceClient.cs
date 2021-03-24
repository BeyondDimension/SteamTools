#if (DEBUG && !UI_DEMO) || (!DEBUG && UI_DEMO)
using System.Application.Models;
using System.Application.Services.CloudService.Clients.Abstractions;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace System.Application.Services.CloudService
{
    public sealed class MockCloudServiceClient : ICloudServiceClient, IAccountClient, IManageClient, IAuthMessageClient, IVersionClient, IActiveUserClient, IAccelerateClient
    {
        public IAccountClient Account => this;
        public IManageClient Manage => this;
        public IAuthMessageClient AuthMessage => this;
        public IVersionClient Version => this;
        public IActiveUserClient ActiveUser => this;
        public IAccelerateClient Accelerate => this;

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

        public Task<IApiResponse<List<ScriptDTO>>> Scripts()
        {
            var list = new List<ScriptDTO>
            {
                new ScriptDTO
                {
                    Name = "GM",
                    Version = "0.1",
                    Author = "软妹币玩家",
                    Description = "基础脚本框架(不建议取消勾选，会导致某些脚本无法运行)",
                },
            };
            return Task.FromResult(ApiResponse.Ok(list));
        }

        public Task<IApiResponse<List<AccelerateProjectGroupDTO>>> All()
        {
            var list = new List<AccelerateProjectGroupDTO>
            {
                new AccelerateProjectGroupDTO
                {
                    Name = "Steam",
                    Items = new List<AccelerateProjectDTO>
                    {
                        new AccelerateProjectDTO
                        {
                            Name = "Steam 社区",
                        },
                        new AccelerateProjectDTO
                        {
                            Name = "Steam 商店",
                        },
                        new AccelerateProjectDTO
                        {
                            Name = "Steam 图片",
                        },
                    },
                },
            };
            return Task.FromResult(ApiResponse.Ok(list));
        }
    }
}
#endif