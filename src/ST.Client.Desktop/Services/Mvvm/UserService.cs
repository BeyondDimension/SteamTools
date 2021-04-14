using Newtonsoft.Json.Linq;
using ReactiveUI;
using System.Application.Models;
using System.Application.Repositories;
using System.Application.UI.Resx;
using System.Application.UI.ViewModels;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;
using System.Windows;
using System.Xml;
using WinAuth;

namespace System.Application.Services
{
    public class UserService : ReactiveObject
    {
        #region static members
        public static UserService Current { get; } = new();
        #endregion

        readonly IUserManager user = DI.Get<IUserManager>();

        public async void ShowWindow(string windowName)
        {
            if (Enum.TryParse<CustomWindow>(windowName, out var @enum))
            {
                switch (@enum)
                {
                    case CustomWindow.LoginOrRegister:
                        var cUser = await user.GetCurrentUserAsync();
                        if (cUser.HasValue()) return;
                        break;
                }
                var vmType = Type.GetType($"System.Application.UI.ViewModels.{@enum}WindowViewModel");
                if (vmType != null && typeof(WindowViewModel).IsAssignableFrom(vmType))
                {
                    await IShowWindowService.Instance.Show(vmType, @enum);
                }
            }
        }


        //public void ShowLoginOrRegisterWindow_Click()
        //{
        //    if (!CurrentUser.HasValue())
        //        DI.Get<IShowWindowService>().Show(CustomWindow.LoginOrRegister, new LoginOrRegisterWindowViewModel());
        //}

        public async void SignOut()
        {
            await user.SignOutAsync();
        }
    }
}
