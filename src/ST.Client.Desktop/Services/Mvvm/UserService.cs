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

        private readonly IUserManager user = DI.Get<IUserManager>();

        public CurrentUser? CurrentUser
        {
            get => user.GetCurrentUser();
        }


        public void ShowLoginOrRegisterWindow_Click()
        {
            if (!CurrentUser.HasValue())
                DI.Get<IShowWindowService>().Show(CustomWindow.LoginOrRegister, new LoginOrRegisterWindowViewModel());
        }

        public void LoginUser_Click()
        {


        }
    }
}
