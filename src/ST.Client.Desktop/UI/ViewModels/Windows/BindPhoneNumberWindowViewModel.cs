using ReactiveUI;
using System.Application.Models;
using System.Application.Services;
using System.Application.UI.Resx;
using System.Collections.Generic;
using System.Properties;
using System.Threading;
using System.Threading.Tasks;
using static System.Application.Services.CloudService.Constants;

namespace System.Application.UI.ViewModels.Windows
{
    public class BindPhoneNumberWindowViewModel : WindowViewModel
    {
        public BindPhoneNumberWindowViewModel() : base()
        {
            Title = ThisAssembly.AssemblyTrademark + " | " + AppResources.User_BindPhoneNum;
        }

        bool _IsLoading;
        public bool IsLoading
        {
            get => _IsLoading;
            set => this.RaiseAndSetIfChanged(ref _IsLoading, value);
        }
    }
}
