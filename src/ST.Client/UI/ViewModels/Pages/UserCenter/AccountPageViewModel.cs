using ReactiveUI;
using System.Application.Services;
using System.Application.UI.Resx;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reactive;
using System.Application.Models;
using System.Windows.Input;

// ReSharper disable once CheckNamespace
namespace System.Application.UI.ViewModels
{
    public partial class AccountPageViewModel
    {
        public ReactiveCommand<Unit, Unit>? OpenUserProfile { get; }

        public ReactiveCommand<Unit, Unit>? OpenEngineOilLogs { get; }

        public ReactiveCommand<Unit, Unit>? OpenBalanceLogs { get; }

        public ReactiveCommand<Unit, Unit>? SignIn { get; }

        public ReactiveCommand<Unit, Unit>? RefreshButton { get; }

        public ReactiveCommand<Unit, Unit>? OpenSteamProfile { get; }

        public ReactiveCommand<Unit, Unit>? OpenNotice { get; }

        public AccountPageViewModel()
        {
            OpenUserProfile = ReactiveCommand.Create(() =>
            {
                UserService.Current.ShowWindow(CustomWindow.UserProfile);
            });
            OpenNotice = ReactiveCommand.Create(() =>
            {
                UserService.Current.ShowWindow(CustomWindow.Notice);
            });
            OpenEngineOilLogs = ReactiveCommand.Create(() =>
            {
            });
            OpenBalanceLogs = ReactiveCommand.Create(() =>
            {
            });
            SignIn = ReactiveCommand.CreateFromTask(async () =>
            {
                var state = await UserService.Current.SignIn();
                if (state.IsSuccess)
                {
                    UserService.Current.User!.Experience = state.Content!.Experience;
                    Toast.Show(AppResources.Account_SignIn_Ok);
                }
                else
                {
                    Toast.Show(state.Message);
                }
            });
            RefreshButton = ReactiveCommand.CreateFromTask(async () =>
            {
                await UserService.Current.RefreshUserAsync();
                Toast.Show(AppResources.RefreshOK);
            });

            OpenSteamProfile = ReactiveCommand.CreateFromTask(async () =>
            {
                if (UserService.Current.User?.SteamAccountId.HasValue ?? false)
                    await Browser2.OpenAsync(string.Format(SteamApiUrls.STEAM_PROFILES_URL, UserService.Current.User!.SteamAccountId));
            });
        }

        /// <summary>
        /// 换绑手机号码 按钮点击
        /// </summary>
        public ICommand OnBtnChangeBindPhoneNumberClick { get; } = ReactiveCommand.Create(() =>
        {
            UserService.Current.ShowWindow(CustomWindow.ChangeBindPhoneNumber);
        });

        /// <summary>
        /// 绑定手机号码 按钮点击
        /// </summary>
        public ICommand OnBtnBindPhoneNumberClick { get; } = ReactiveCommand.Create(() =>
        {
            UserService.Current.ShowWindow(CustomWindow.BindPhoneNumber);
        });

        public override async void Activation()
        {
            if (IsFirstActivation && NotificationService.Current.NoticeTypes.Count == 0)
                await NotificationService.Current.InitializeNotice();
            var defaultNotice = NotificationService.Current?.NoticeTypes.Items.FirstOrDefault();
            if (defaultNotice != null && NotificationService.Current?.SelectGroup?.Id != defaultNotice.Id)
                await NotificationService.Current!.GetTable(defaultNotice!);
            base.Activation();
        }

        public bool IsSponsor => UserService.Current.User?.UserType.HasFlag(UserType.Sponsor) ?? false;

        public double LinearWidth => 25 * Math.Round((double)(UserService.Current.User?.Experience ?? 0) / (UserService.Current.User?.NextExperience ?? 0), 2);

    }
}
