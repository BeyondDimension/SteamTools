using Android.Views;
using Android.Widget;
using Binding;
using ReactiveUI;
using System.Application.Services;
using System.Application.UI.Resx;
using System.Application.UI.ViewModels;

namespace System.Application.UI.Fragments
{
    [Authorize]
    internal sealed class UserBasicInfoFragment : BaseFragment<fragment_basic_profile, UserProfileWindowViewModel>, DatePickerHelper.IOnPositiveButtonClickListener
    {
        protected override int? LayoutResource => Resource.Layout.fragment_basic_profile;

        void SetPhoneNumberBindOrModifyText(bool hasPhoneNumber)
        {
            binding!.btnModifyPhoneNumber.Text = hasPhoneNumber ? AppResources.Modify : AppResources.Bind;
        }

        public override void OnCreateView(View view)
        {
            base.OnCreateView(view);

            binding!.layoutNickName.SetMaxLength(ModelValidatorLengths.NickName);
            binding.tbNickName.SetMaxLength(ModelValidatorLengths.NickName);

            // Gender 双向绑定
            ViewModel!.WhenAnyValue(x => x.Gender).SubscribeInMainThread(value =>
            {
                Gender = value;
            }).AddTo(this);
            binding.rgGender.CheckedChange += RgGender_CheckedChange;

            binding.tbPhoneNumber.Text = UserService.Current.PhoneNumber;

            R.Subscribe(() =>
            {
                if (binding == null) return;
                binding.btnUploadYourAvatar.Text = AppResources.UploadYourAvatar;
                binding.tvUploadYourAvatarDesc.Text = $"{AppResources.UploadYourAvatarDesc}{Environment.NewLine}{AppResources.UnderConstruction}";
                binding.layoutNickName.Hint = AppResources.NickName;
                binding.layoutPhoneNumber.Hint = AppResources.User_Phone;
                SetPhoneNumberBindOrModifyText(UserService.Current.HasPhoneNumber);
                binding.layoutArea2.Hint = AppResources.UserProfile_Location;
                binding.layoutGender.Hint = AppResources.Gender;
                binding.layoutBirthDate.Hint = AppResources.UserProfile_BirthDate;
                binding.btnSubmit.Text = AppResources.SaveChanges;
                binding.rbGender0.Text = AppResources.Gender_Unknown;
                binding.rbGender1.Text = AppResources.Gender_Male;
                binding.rbGender2.Text = AppResources.Gender_Female;
                binding.btnSubmit.Text = AppResources.SaveChanges;
            }).AddTo(this);

            // NickName 双向绑定
            ViewModel!.WhenAnyValue(x => x.NickName).SubscribeInMainThread(value =>
            {
                if (binding == null) return;
                if (value != binding.tbNickName.Text) binding.tbNickName.Text = value;
            }).AddTo(this);
            binding.tbNickName.TextChanged += (_, _) =>
            {
                if (binding == null || ViewModel == null) return;
                var text = binding.tbNickName.Text;
                if (text != ViewModel.NickName) ViewModel.NickName = text;
            };

            UserService.Current.WhenAnyValue(x => x.HasPhoneNumber).SubscribeInMainThread(value =>
            {
                SetPhoneNumberBindOrModifyText(value);
            }).AddTo(this);

            ViewModel!.WhenAnyValue(x => x.IsModify).SubscribeInMainThread(value =>
            {
                if (binding == null) return;
                binding.btnSubmit.Enabled = value;
            }).AddTo(this);

            SetOnClickListener(
                binding.btnModifyPhoneNumber,
                binding.clickBirthDate,
                binding.btnSubmit);
        }

        /// <summary>
        /// 设置或获取当前 UI 上的性别 Radio
        /// </summary>
        public Gender Gender
        {
            get
            {
                if (binding != null)
                {
                    var id = binding.rgGender.CheckedRadioButtonId;
                    if (id == Resource.Id.rbGender1)
                    {
                        return Gender.Male;
                    }
                    else if (id == Resource.Id.rbGender2)
                    {
                        return Gender.Female;
                    }
                }
                return Gender.Unknown;
            }
            set
            {
                if (binding != null)
                {
                    var id = value switch
                    {
                        Gender.Male => Resource.Id.rbGender1,
                        Gender.Female => Resource.Id.rbGender2,
                        _ => Resource.Id.rbGender0,
                    };
                    if (id != binding.rgGender.CheckedRadioButtonId)
                    {
                        binding.rgGender.Check(id);
                    }
                }
            }
        }

        private void RgGender_CheckedChange(object sender, RadioGroup.CheckedChangeEventArgs e)
        {
            if (e == null || binding == null || ViewModel == null) return;
            if (e.CheckedId == Resource.Id.rbGender0)
            {
                binding.ivIconGender.SetImageResource(Resource.Drawable.
                    baseline_visibility_off_black_24);
                ViewModel.Gender = Gender.Unknown;
            }
            else if (e.CheckedId == Resource.Id.rbGender1)
            {
                binding.ivIconGender.SetImageResource(Resource.Drawable.
                    baseline_male_black_24);
                ViewModel.Gender = Gender.Male;
            }
            else if (e.CheckedId == Resource.Id.rbGender2)
            {
                binding.ivIconGender.SetImageResource(Resource.Drawable.
                    baseline_female_black_24);
                ViewModel.Gender = Gender.Female;
            }
        }

        protected override bool OnClick(View view)
        {
            if (ViewModel != null)
            {
                if (view.Id == Resource.Id.btnModifyPhoneNumber)
                {
                    if (UserService.Current.HasPhoneNumber)
                    {
                        ViewModel.OnBtnChangeBindPhoneNumberClick.Invoke();
                    }
                    else
                    {
                        ViewModel.OnBtnBindPhoneNumberClick.Invoke();
                    }
                    return true;
                }
                else if (view.Id == Resource.Id.clickBirthDate)
                {
                    this.ShowBirthDatePicker(ViewModel.BirthDate?.DateTime ?? default);
                    return true;
                }
                else if (view.Id == Resource.Id.btnSubmit)
                {
                    ViewModel.Submit();
                    return true;
                }
            }
            return base.OnClick(view);
        }

        void DatePickerHelper.IOnPositiveButtonClickListener.OnPositiveButtonClick(DateTimeOffset selection)
        {
            if (ViewModel == null) return;
            ViewModel.BirthDate = selection;
        }
    }
}
