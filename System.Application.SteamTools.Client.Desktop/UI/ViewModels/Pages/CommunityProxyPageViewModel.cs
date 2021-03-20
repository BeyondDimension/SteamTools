using System.Application.UI.Resx;
using System.Collections.Generic;

namespace System.Application.UI.ViewModels
{
    public class CommunityProxyPageViewModel : TabItemViewModel
    {
        public override string Name
        {
            get => AppResources.CommunityFix;
            protected set { throw new NotImplementedException(); }
        }

        //private IList<MenuItemViewModel> _MenuItems = new[]
        //{
        //        new MenuItemViewModel
        //        {
        //            Header = AppResources.More,
        //            Items = new[]
        //    {
        //        new MenuItemViewModel { Header = "_登录新账号",IconKey="SteamDrawing" },
        //        new MenuItemViewModel { Header = "编辑" },
        //        new MenuItemViewModel { Header = "-" },
        //        new MenuItemViewModel
        //        {
        //            Header = "Recent",
        //            Items = new[]
        //            {
        //                new MenuItemViewModel
        //                {
        //                    Header = "File1.txt",
        //                },
        //                new MenuItemViewModel
        //                {
        //                    Header = "File2.txt",
        //                },
        //            }
        //        },
        //    }
        //        },
        //};


    }
}
