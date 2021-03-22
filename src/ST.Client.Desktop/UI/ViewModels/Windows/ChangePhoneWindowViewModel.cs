using Newtonsoft.Json.Linq;
using System.Application.Models;
using System.Application.UI.Resx;
using System.Properties;

namespace System.Application.UI.ViewModels
{
    public class ChangePhoneWindow : WindowViewModel
    {
        public ChangePhoneWindow()
        {
            Title = ThisAssembly.AssemblyTrademark + " | " + AppResources.User_ChangePhoneNum;
        }
    }
}