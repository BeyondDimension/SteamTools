#if (DEBUG && !UI_DEMO) || (!DEBUG && UI_DEMO)
using System.Application.Models;
using System.Application.Services.CloudService.Clients.Abstractions;
using System.Collections.Generic;
using System.IO.FileFormats;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace System.Application.Services.CloudService
{
    public sealed partial class MockCloudServiceClient : ICloudServiceClient, IAccountClient, IManageClient, IAuthMessageClient, IVersionClient, IActiveUserClient, IAccelerateClient, ISteamCommunityClient
    {
        readonly IToast toast;
        readonly IModelValidator validator;
        readonly ICloudServiceClient real;

        public MockCloudServiceClient(IToast toast, IModelValidator validator, CloudServiceClientBase real)
        {
            this.toast = toast;
            this.validator = validator;
            this.real = real;
        }

        public string ApiBaseUrl => real.ApiBaseUrl;
        public IAccountClient Account => this;
        public IManageClient Manage => this;
        public IAuthMessageClient AuthMessage => this;
        public IVersionClient Version => this;
        public IActiveUserClient ActiveUser => this;
        public IAccelerateClient Accelerate => this;
        public ISteamCommunityClient SteamCommunity => this;

        #region ModelValidator

        IApiResponse? ModelValidator<TRequestModel>(TRequestModel requestModel) => ModelValidator<TRequestModel, object>(requestModel);

        IApiResponse<TResponseModel>? ModelValidator<TRequestModel, TResponseModel>(TRequestModel requestModel)
        {
            if (requestModel != null && typeof(TRequestModel) != typeof(object))
            {
                if (!validator.Validate(requestModel, out var errorMessage))
                {
                    return ApiResponse.Code<TResponseModel>(
                        ApiResponseCode.RequestModelValidateFail, errorMessage);
                }
            }
            return null;
        }

        #endregion

        void ShowResponseErrorMessage(IApiResponse response)
        {
            if (response.Code == ApiResponseCode.Canceled) return;
            var message = ApiResponse.GetMessage(response);
            toast.Show(message);
        }

        void GlobalResponseIntercept(IApiResponse response)
        {
            if (!response.IsSuccess)
            {
                ShowResponseErrorMessage(response);
            }
        }

        public async Task<IApiResponse<string>> ChangeBindPhoneNumber(ChangePhoneNumberRequest.Validation request)
        {
            var mockDelay = false;
            var rsp = ModelValidator<ChangePhoneNumberRequest.Validation, string>(request) ?? ChangeBindPhoneNumber_();
            if (mockDelay) await Task.Delay(1500);
            GlobalResponseIntercept(rsp);
            return rsp;
            IApiResponse<string> ChangeBindPhoneNumber_()
            {
                mockDelay = true;
                return ApiResponse.Ok("123");
            }
        }

        public async Task<IApiResponse> ChangeBindPhoneNumber(ChangePhoneNumberRequest.New request)
        {
            var mockDelay = false;
            var rsp = ModelValidator(request) ?? ChangeBindPhoneNumber_();
            if (mockDelay) await Task.Delay(1500);
            GlobalResponseIntercept(rsp);
            return rsp;
            IApiResponse ChangeBindPhoneNumber_()
            {
                mockDelay = true;
                return ApiResponse.Ok();
            }
        }

        public Task<IApiResponse<AppVersionDTO?>> CheckUpdate(Guid id, Platform platform, DeviceIdiom deviceIdiom, ArchitectureFlags supportedAbis, Version osVersion, ArchitectureFlags abi)
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
            return real.Forward(request, completionOption, cancellationToken);
        }

        public async Task<IApiResponse<LoginOrRegisterResponse>> LoginOrRegister(LoginOrRegisterRequest request)
        {
            var mockDelay = false;
            var rsp = ModelValidator<LoginOrRegisterRequest, LoginOrRegisterResponse>(request) ?? LoginOrRegister_();
            if (mockDelay) await Task.Delay(1500);
            GlobalResponseIntercept(rsp);
            return rsp;
            IApiResponse<LoginOrRegisterResponse> LoginOrRegister_()
            {
                mockDelay = true;
                return ApiResponse.Ok(new LoginOrRegisterResponse
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
                });
            }
        }

        public Task<IApiResponse<NotificationRecordDTO?>> Post(ActiveUserRecordDTO record, Guid? lastNotificationRecordId)
        {
            return Task.FromResult(ApiResponse.Ok<NotificationRecordDTO?>(null));
        }

        public Task<IApiResponse<JWTEntity>> RefreshToken(string refresh_token)
        {
            throw new NotImplementedException();
        }

        public ValueTask<IApiResponse> SendSms(SendSmsRequest request)
        {
            var rsp = ModelValidator(request) ?? ApiResponse.Ok();
            GlobalResponseIntercept(rsp);
            return new ValueTask<IApiResponse>(rsp);
        }

        public Task<IApiResponse<SteamMiniProfile>> MiniProfile(int steamId32)
        {
            var content = new SteamMiniProfile
            {
                Nameplate = new List<SteamMiniProfile.Nameplate_>
                {
                    new SteamMiniProfile.Nameplate_
                    {
                        Format = VideoFormat.WebM,
                        Src = "https://media.st.dl.pinyuncloud.com/steamcommunity/public/images/items/1504020/19511e3bc7b3d248b82cabdde810e7aea3d2b6f1.webm",
                    },
                    new SteamMiniProfile.Nameplate_
                    {
                        Format = VideoFormat.MP4,
                        Src = "https://media.st.dl.pinyuncloud.com/steamcommunity/public/images/items/1504020/74ded39af957315c5bba17202489bbed570135ec.mp4",
                    },
                },
                PlayerSection = new SteamMiniProfile.PlayerSection_
                {
                    AvatarFrame = "https://media.st.dl.pinyuncloud.com/steamcommunity/public/images/items/212070/9b6b26c7a03046da283408d72319f9eec932c80a.gif",
                    Avatar = "https://media.st.dl.pinyuncloud.com/steamcommunity/public/images/items/1504020/bc6fc1f46697d79a8add0e30862d74dbaf50cc4d.gif",
                    Persona = "RuaRua",
                    FriendStatus = "在线",
                },
                Detailssection = new SteamMiniProfile.Detailssection_
                {
                    Badge = "https://community.akamai.steamstatic.com/public/images/badges/26_summer2017_sticker/completionist.png",
                    BadgeName = "贴纸完满主义者",
                    BadgeXp = "100 点经验值",
                    PlayerLevel = ushort.MaxValue,
                },
            };
            var rsp = ApiResponse.Ok(content);
            return Task.FromResult(rsp);
        }
    }
}
#endif