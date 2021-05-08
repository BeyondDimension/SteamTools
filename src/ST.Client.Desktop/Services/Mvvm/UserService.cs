using ReactiveUI;
using System.Application.Models;
using System.Application.UI.ViewModels;
using System.Reactive;
using System.Threading.Tasks;
using System.Windows.Input;

namespace System.Application.Services
{
    public class UserService : ReactiveObject
    {
        #region static members
        public static UserService Current { get; } = new();
        #endregion

        readonly IUserManager userManager = DI.Get<IUserManager>();

        public async void ShowWindow(CustomWindow windowName)
        {
            var isDialog = true;
            if (windowName == CustomWindow.LoginOrRegister)
            {
                var cUser = await userManager.GetCurrentUserAsync();
                if (cUser.HasValue()) return;
                isDialog = false;
            }
            else if (windowName == CustomWindow.ChangeBindPhoneNumber)
            {
                var cUser = await userManager.GetCurrentUserAsync();
                if (!cUser.HasValue()) return;
                if (string.IsNullOrWhiteSpace(cUser!.PhoneNumber)) return;
            }
            var vmType = Type.GetType($"System.Application.UI.ViewModels.{windowName}WindowViewModel");
            if (vmType != null && typeof(WindowViewModel).IsAssignableFrom(vmType))
            {
                await IShowWindowService.Instance.ShowDialog(vmType, windowName, isDialog: isDialog);
            }
        }

        public void SignOut()
        {
            SignOutApi();
            SignOutUserManager();
        }

        public async void SignOutUserManager()
        {
            await userManager.SignOutAsync();
        }

        public async void SignOutApi()
        {
            await ICloudServiceClient.Instance.Manage.SignOut();
        }

        UserInfoDTO? _User;
        public UserInfoDTO? User
        {
            get => _User;
            set => this.RaiseAndSetIfChanged(ref _User, value);
        }

        SteamUser? _SteamUser;
        public SteamUser? CurrentSteamUser
        {
            get => _SteamUser;
            set => this.RaiseAndSetIfChanged(ref _SteamUser, value);
        }

        object GetAvaterPath(UserInfoDTO? user)
        {
            object? value = null;
            if (user is UserInfoDTO userInfo && userInfo.SteamAccountId.HasValue)
            {
                // Steam Avatar
                value = CurrentSteamUser?.AvatarStream;
            }

            if (user is IUserDTO user2 && user2.Avatar.HasValue)
            {
                // Guid Avatar
                value = ImageUrlHelper.GetImageApiUrlById(user2.Avatar.Value);
            }
            return value ?? DefaultAvaterPath;
        }

        const string DefaultAvaterPath = "avares://System.Application.SteamTools.Client.Desktop.Avalonia/Application/UI/Assets/AppResources/avater_default.png";

        object? _AvaterPath = DefaultAvaterPath;

        public object? AvaterPath
        {
            get => _AvaterPath;
            set => this.RaiseAndSetIfChanged(ref _AvaterPath, value);
        }

        public UserService()
        {
            this.WhenAnyValue(x => x.User)
                  .Subscribe(_ => this.RaisePropertyChanged(nameof(AvaterPath)));

            userManager.OnSignOut += () =>
            {
                User = null;
                CurrentSteamUser = null;
                AvaterPath = DefaultAvaterPath;
            };

            //ShowWindow = ReactiveCommand.Create<CustomWindow>(n => ShowWindowF(n));

            Task.Run(Initialize).ForgetAndDispose();
        }

        //public ICommand ShowWindow { get; }

        async void Initialize()
        {
            await RefreshUserAsync();
        }

        bool _HasPhoneNumber;
        public bool HasPhoneNumber
        {
            get => _HasPhoneNumber;
            set => this.RaiseAndSetIfChanged(ref _HasPhoneNumber, value);
        }

        public async Task RefreshUserAsync()
        {
            User = await userManager.GetCurrentUserInfoAsync();

            if (User != null && User.SteamAccountId.HasValue)
            {
                CurrentSteamUser = await ISteamworksWebApiService.Instance.GetUserInfo(User.SteamAccountId.Value);
                CurrentSteamUser.AvatarStream = IHttpService.Instance.GetImageAsync(CurrentSteamUser.AvatarFull, ImageChannelType.SteamAvatars);
                AvaterPath = new CircleImageStream(await CurrentSteamUser.AvatarStream);
            }

            //AvaterPath = GetAvaterPath(User);

            var userInfo = await userManager.GetCurrentUserAsync();
            HasPhoneNumber = !string.IsNullOrWhiteSpace(userInfo?.PhoneNumber);
        }
    }
}