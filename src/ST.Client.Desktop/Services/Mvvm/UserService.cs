using ReactiveUI;
using System.Application.Models;
using System.Application.UI.ViewModels;
using System.Threading.Tasks;

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
            switch (windowName)
            {
                case CustomWindow.LoginOrRegister:
                    var cUser = await userManager.GetCurrentUserAsync();
                    if (cUser.HasValue()) return;
                    break;
            }
            var vmType = Type.GetType($"System.Application.UI.ViewModels.{windowName}WindowViewModel");
            if (vmType != null && typeof(WindowViewModel).IsAssignableFrom(vmType))
            {
                await IShowWindowService.Instance.Show(vmType, windowName);
            }
        }

        public async void SignOut()
        {
            await userManager.SignOutAsync();
        }

        UserInfoDTO? _User;
        public UserInfoDTO? User
        {
            get => _User;
            set => this.RaiseAndSetIfChanged(ref _User, value);
        }

        static string GetAvaterPath(UserInfoDTO? user)
        {
            if (user is UserInfoDTO userInfo && userInfo.SteamAccountId.HasValue)
            {
                // Steam Avatar
            }

            if (user is IUserDTO user2 && user2.Avatar.HasValue)
            {
                // Guid Avatar
                return ImageUrlHelper.GetImageApiUrlById(user2.Avatar.Value);
            }
            return DefaultAvaterPath;
        }

        const string DefaultAvaterPath = "avares://System.Application.SteamTools.Client.Desktop.Avalonia/Application/UI/Assets/AppResources/avater_default.png";
        string _AvaterPath = DefaultAvaterPath;
        public string AvaterPath
        {
            get => GetAvaterPath(User);
            set => this.RaiseAndSetIfChanged(ref _AvaterPath, value);
        }

        public UserService()
        {
            this.WhenAnyValue(x => x.User)
                  .Subscribe(x => AvaterPath = GetAvaterPath(x));

            userManager.OnSignOut += () =>
            {
                User = null;
            };
            Task.Run(Initialize).ForgetAndDispose();
        }

        async void Initialize()
        {
            await RefreshUserAsync();
        }

        public async Task RefreshUserAsync()
        {
            User = await userManager.GetCurrentUserInfoAsync();
            AvaterPath = GetAvaterPath(User);
        }
    }
}