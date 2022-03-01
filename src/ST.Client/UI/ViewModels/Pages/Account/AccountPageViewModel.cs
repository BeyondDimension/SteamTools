using System;
using System.Application.Services;
using System.Application.UI.Resx;
using System.Collections.Generic;
using System.Linq;
using System.Text;

// ReSharper disable once CheckNamespace
namespace System.Application.UI.ViewModels
{
    public partial class AccountPageViewModel
    {
        public AccountPageViewModel()
        {

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
