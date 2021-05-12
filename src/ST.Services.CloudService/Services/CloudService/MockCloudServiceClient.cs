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
    public sealed partial class MockCloudServiceClient : ICloudServiceClient, IAccountClient, IManageClient, IAuthMessageClient, IVersionClient, IActiveUserClient, IAccelerateClient, IScriptClient
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
        public IScriptClient Script => this;
        public IManageClient Manage => this;
        public IAuthMessageClient AuthMessage => this;
        public IVersionClient Version => this;
        public IActiveUserClient ActiveUser => this;
        public IAccelerateClient Accelerate => this;

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

        public async Task<IApiResponse> BindPhoneNumber(BindPhoneNumberRequest request)
        {
            var mockDelay = false;
            var rsp = ModelValidator(request) ?? BindPhoneNumber_();
            if (mockDelay) await Task.Delay(1500);
            GlobalResponseIntercept(rsp);
            return rsp;
            IApiResponse BindPhoneNumber_()
            {
                mockDelay = true;
                return ApiResponse.Ok();
            }
        }

        public Task<IApiResponse<AppVersionDTO?>> CheckUpdate(Guid id, Platform platform, DeviceIdiom deviceIdiom, ArchitectureFlags supportedAbis, Version osVersion, ArchitectureFlags abi)
        {
            return Task.FromResult(ApiResponse.Ok<AppVersionDTO?>(default));
        }

        public Task<IApiResponse> UnbundleAccount(FastLoginChannel channel)
        {
            return Task.FromResult(ApiResponse.Ok());
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

        public Task<IApiResponse> Download(bool isAnonymous, string requestUri, string cacheFilePath, IProgress<float>? progress, CancellationToken cancellationToken = default)
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

        public async Task<IApiResponse> EditUserProfile(EditUserProfileRequest request)
        {
            await Task.Delay(1500);
            return ApiResponse.Ok();
        }

        public async Task<IApiResponse<ScriptResponse>> Basics(string? msg = null)
        {
            await Task.Delay(1500);
            return ApiResponse.Ok(new ScriptResponse
            {
                Version = "00.1"
            });
        }

        public async Task<IApiResponse<PagedModel<ScriptDTO>>> ScriptTable(string? name = null, int pageIndex = 1, int pageSize = 15, string? msg = null)
        {
            await Task.Delay(1500);
            return ApiResponse.Ok(new PagedModel<ScriptDTO> { });
        }

        public async Task<IApiResponse<IList<ScriptResponse>>> ScriptUpdateInfo(IEnumerable<Guid> ids, string? msg = null)
        {
            await Task.Delay(1500);
            return ApiResponse.Ok(new List<ScriptResponse> { });
        }

        public async Task<IApiResponse> SignOut()
        {
            await Task.Delay(1500);
            return ApiResponse.Ok();
        }
    }
}
#endif