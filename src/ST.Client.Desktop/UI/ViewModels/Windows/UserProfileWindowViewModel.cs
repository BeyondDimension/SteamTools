using ReactiveUI;
using System.Application.Entities;
using System.Application.Models;
using System.Application.Services;
using System.Application.UI.Resx;
using System.Linq;
using System.Linq.Expressions;
using System.Properties;

namespace System.Application.UI.ViewModels
{
    public partial class UserProfileWindowViewModel : WindowViewModel
    {
        byte[]? userInfoValue;
        UserInfoDTO? userInfoSource;
        public UserProfileWindowViewModel()
        {
            Title = ThisAssembly.AssemblyTrademark + " | " + AppResources.UserProfile;
            Initialize();
            async void Initialize()
            {
                var userManager = DI.Get<IUserManager>();
                userInfoSource = await userManager.GetCurrentUserInfoAsync() ?? new UserInfoDTO();
                userInfoValue = Serializable.SMP(userInfoSource);

                // (SetFields)DTO => VM
                _NickName = userInfoSource.NickName;
                _Gender = userInfoSource.Gender;
                _BirthDate = userInfoSource.BirthDate.HasValue ?
                    new DateTimeOffset(userInfoSource.BirthDate.Value.Year,
                    userInfoSource.BirthDate.Value.Month,
                    userInfoSource.BirthDate.Value.Day,
                    0, 0, 0,
                    new TimeSpan(userInfoSource.TimeZoneTicks)) : null;
                if (userInfoSource.AreaId.HasValue) this.SetAreaId(userInfoSource.AreaId.Value);

                userInfoSource = Serializable.DMP<UserInfoDTO>(userInfoValue) ?? throw new ArgumentNullException(nameof(userInfoSource));
                CurrentPhoneNumber = await userManager.GetCurrentUserPhoneNumberAsync();
                this.InitAreas();

                // IsModifySubscribe
                void SubscribeOnNext<T>(T value, Action<T> onNext)
                {
                    onNext(value);
                    var currentUserInfoValue = Serializable.SMP(userInfoSource);
                    IsModify = !Enumerable.SequenceEqual(userInfoValue, currentUserInfoValue);
                }
                void SubscribeFormItem<T>(Expression<Func<UserProfileWindowViewModel, T>> expression, Action<T> onNext) => this.WhenAnyValue(expression).Subscribe(value => SubscribeOnNext(value, onNext)).AddTo(this);
                SubscribeFormItem(x => x.NickName, x => userInfoSource.NickName = x ?? string.Empty);
                SubscribeFormItem(x => x.Gender, x => userInfoSource.Gender = x);
                SubscribeFormItem(x => x.BirthDate, x => userInfoSource.BirthDate = x.HasValue ?
                    new DateTime(x.Value.Year, x.Value.Month, x.Value.Day,
                    0, 0, 0, userInfoSource.BirthDate?.Kind ?? DateTimeKind.Unspecified) : null);
                void SubscribeAreaOnNext(IArea? area)
                {
                    var areaId = area?.Id;
                    userInfoSource.AreaId = (!areaId.HasValue ||
                        areaId.Value == AreaUIHelper.PleaseSelect.Id) ?
                        this.GetSelectAreaId() : areaId;
                }
                SubscribeFormItem(x => x.AreaSelectItem2, SubscribeAreaOnNext);
                SubscribeFormItem(x => x.AreaSelectItem3, SubscribeAreaOnNext);
                SubscribeFormItem(x => x.AreaSelectItem4, SubscribeAreaOnNext);
            }
        }

        bool _IsModify;
        public bool IsModify
        {
            get => _IsModify;
            set => this.RaiseAndSetIfChanged(ref _IsModify, value);
        }

        bool _IsLoading;
        public bool IsLoading
        {
            get => _IsLoading;
            set => this.RaiseAndSetIfChanged(ref _IsLoading, value);
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

        DateTimeOffset? _BirthDate;
        public DateTimeOffset? BirthDate
        {
            get => _BirthDate;
            set => this.RaiseAndSetIfChanged(ref _BirthDate, value);
        }

        public void Submit()
        {
            if (!IsModify) return;

            Toast.Show($"Gender: {Gender.ToStringDisplay()}, AreaId: {this.GetSelectAreaId()}");
        }
    }
}