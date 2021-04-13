using ReactiveUI;
using System.Application.Services;
using System.Application.UI.Resx;
using System.Properties;

namespace System.Application.UI.ViewModels
{
    public partial class UserProfileWindowViewModel : WindowViewModel
    {
        public UserProfileWindowViewModel()
        {
            Title = ThisAssembly.AssemblyTrademark + " | " + AppResources.UserProfile;
            Initialize();
        }

        public async void Initialize()
        {
            CurrentPhoneNumber = await DI.Get<IUserManager>().GetCurrentUserPhoneNumberAsync();
            this.InitAreas();
        }

        string? _NickName;
        public string? NickName
        {
            get => _NickName;
            set => this.RaiseAndSetIfChanged(ref _NickName, value);
        }

        string? _CurrentPhoneNumber;
        public string? CurrentPhoneNumber
        {
            get => _CurrentPhoneNumber;
            set => this.RaiseAndSetIfChanged(ref _CurrentPhoneNumber, value);
        }

        Gender _Gender;
        public Gender Gender
        {
            get => _Gender;
            set => this.RaiseAndSetIfChanged(ref _Gender, value);
        }

        public void Submit()
        {
            Toast.Show($"Gender: {Gender.ToStringDisplay()}, AreaId: {this.GetSelectAreaId()}");
        }
    }
}