using DynamicData;
using ReactiveUI;
using System.Application.Models;
using System.Application.Repositories;
using System.Application.Services;
using System.Application.UI.Resx;
using System.Collections.ObjectModel;
using System.Linq;
using System.Properties;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using WinAuth;
using MyAuthenticator = System.Application.Models.Abstractions.MyAuthenticator;

namespace System.Application.UI.ViewModels
{
    public partial class AuthTradeWindowViewModel : Abstractions.AuthTradeWindowViewModel
    {
        public AuthTradeWindowViewModel() : base()
        {

        }

        public AuthTradeWindowViewModel(MyAuthenticator? auth) : base(auth)
        {

        }
    }
}