using System.Application.Models;
using System.Application.UI.Resx;
using System.Properties;
using MyAuthenticator = System.Application.Models.Abstractions.MyAuthenticator;

namespace System.Application.UI.ViewModels
{
    public partial class ShowAuthWindowViewModel : Abstractions.ShowAuthWindowViewModel
    {
        public ShowAuthWindowViewModel() : base()
        {

        }

        public ShowAuthWindowViewModel(MyAuthenticator? auth) : base(auth)
        {

        }
    }
}