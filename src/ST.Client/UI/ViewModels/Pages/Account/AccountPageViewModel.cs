using ReactiveUI;
using System;
using System.Application.Services;
using System.Application.UI.Resx;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reactive;

// ReSharper disable once CheckNamespace
namespace System.Application.UI.ViewModels
{
    public partial class AccountPageViewModel
    {
        public ReactiveCommand<Unit, Unit>? OpenUserProfile { get; }
        public ReactiveCommand<Unit, Unit>? OpenEngineOilLogs { get; }
        public ReactiveCommand<Unit, Unit>? OpenBalanceLogs { get; }
        public AccountPageViewModel()
        {
            OpenUserProfile = ReactiveCommand.Create(() =>
            {
                UserService.Current.ShowWindow(CustomWindow.UserProfile);
            });
            OpenEngineOilLogs = ReactiveCommand.Create(() =>
            { 
            });
            OpenBalanceLogs = ReactiveCommand.Create(() =>
            { 
            });
        }
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

    }
}
