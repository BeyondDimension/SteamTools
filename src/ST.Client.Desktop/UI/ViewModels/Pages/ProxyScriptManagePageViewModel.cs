using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;
using ReactiveUI;
using System.Application.Models;
using System.Application.Repositories;
using System.Application.Services;
using System.Application.UI.Resx;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Net.NetworkInformation;
using System.Security;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using static Newtonsoft.Json.JsonConvert;

namespace System.Application.UI.ViewModels
{
    public class ProxyScriptManagePageViewModel : TabItemViewModel
    {
        public override string Name
        {
            get => AppResources.ScriptConfig;
            protected set { throw new NotImplementedException(); }
        }

        public ProxyScriptManagePageViewModel()
        {
            MenuItems = new ObservableCollection<MenuItemViewModel>()
            {
                   new MenuItemViewModel (nameof(AppResources.CommunityFix_EnableScriptService)),
                   new MenuItemViewModel (nameof(AppResources.CommunityFix_ScriptManage)),
            };

        }
    }
}