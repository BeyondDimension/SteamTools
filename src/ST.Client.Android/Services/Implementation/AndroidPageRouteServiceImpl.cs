using System;
using System.Application.UI.ViewModels;
using System.Collections.Generic;
using System.Text;
using XEPlatform = Xamarin.Essentials.Platform;
using static System.Application.UI.ViewModels.TabItemViewModel;
using System.Application.UI.Activities;

namespace System.Application.Services.Implementation
{
    internal sealed class AndroidPageRouteServiceImpl : IPlatformPageRouteService
    {
        bool IPlatformPageRouteService.IsUseNativePage(TabItemId tabItemId) => false;
        //=> tabItemId switch
        //{
        //    TabItemId.Settings or TabItemId.About => true,
        //    _ => false,
        //};

        void IPlatformPageRouteService.GoToNativePage(TabItemId tabItemId) { }
        //=> GoToNativePage(tabItemId switch
        //{
        //    TabItemId.Settings => typeof(SettingsActivity),
        //    TabItemId.About => typeof(AboutActivity),
        //    _ => null,
        //});

        void GoToNativePage(Type? activityType)
        {
            if (activityType == null) return;
            XEPlatform.CurrentActivity?.StartActivity(activityType);
        }
    }
}
