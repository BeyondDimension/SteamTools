using Android.Runtime;
using Android.Views;
using Android.Widget;
using Binding;
using ReactiveUI;
using System.Application.Entities;
using System.Application.Services;
using System.Application.UI.Resx;
using System.Application.UI.ViewModels;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

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

            ViewModel!.Close = () => RequireActivity().Finish();

            // Gender 双向绑定
            UIGender = ViewModel.Gender;
            ViewModel.WhenAnyValue(x => x.Gender).SubscribeInMainThread(value =>
            {
                if (gender != value)
                {
                    UIGender = value;
                }
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

            #region Area ComboBox

            var areaItem2 = ComboBoxHelper.CreateArrayAdapter<IArea>(binding.tbArea2);
            var areaItem3 = ComboBoxHelper.CreateArrayAdapter<IArea>(binding.tbArea3);
            var areaItem4 = ComboBoxHelper.CreateArrayAdapter<IArea>(binding.tbArea4);
            static Action<IReadOnlyList<IArea>?> AreaDelegate(ArrayAdapter<IArea> adapter) => value =>
            {
                adapter.Clear();
                if (value != null)
                {
                    adapter.AddAll(value.ToJavaCollectionNoGeneric());
                }
                adapter.NotifyDataSetChanged();
            };
            ViewModel!.WhenAnyValue(x => x.AreaItems2)
                .SubscribeInMainThread(AreaDelegate(areaItem2)).AddTo(this);
            ViewModel!.WhenAnyValue(x => x.AreaItems3)
                .SubscribeInMainThread(AreaDelegate(areaItem3)).AddTo(this);
            ViewModel!.WhenAnyValue(x => x.AreaItems4)
                .SubscribeInMainThread(AreaDelegate(areaItem4)).AddTo(this);
            static IArea? GetArea(string? str, IReadOnlyList<IArea>? areas) => string.IsNullOrWhiteSpace(str) ? null : areas?.FirstOrDefault(x => x.ToString() == str);
            binding.tbArea2.TextChanged += (_, _) =>
            {
                var text = binding.tbArea2.Text;
                ViewModel!.AreaSelectItem2 = GetArea(text, ViewModel.AreaItems2);
            };
            binding.tbArea3.TextChanged += (_, _) =>
            {
                var text = binding.tbArea3.Text;
                ViewModel!.AreaSelectItem3 = GetArea(text, ViewModel.AreaItems3);
            };
            binding.tbArea4.TextChanged += (_, _) =>
            {
                var text = binding.tbArea4.Text;
                ViewModel!.AreaSelectItem4 = GetArea(text, ViewModel.AreaItems4);
            };

            #endregion

            SetOnClickListener(
                binding.btnModifyPhoneNumber,
                binding.clickBirthDate,
                binding.btnSubmit);
        }

        Gender gender;
        /// <summary>
        /// 设置或获取当前 UI 上的性别 Radio
        /// </summary>
        Gender UIGender
        {
            get
            {
                return gender;
                //if (binding != null)
                //{
                //    var id = binding.rgGender.CheckedRadioButtonId;
                //    if (id == Resource.Id.rbGender1)
                //    {
                //        return Gender.Male;
                //    }
                //    else if (id == Resource.Id.rbGender2)
                //    {
                //        return Gender.Female;
                //    }
                //}
                //return Gender.Unknown;
            }
            set
            {
                if (gender == value) return;
                gender = value;
                if (binding != null)
                {
                    var id = value switch
                    {
                        Gender.Male => Resource.Id.rbGender1,
                        Gender.Female => Resource.Id.rbGender2,
                        _ => Resource.Id.rbGender0,
                    };
                    var c_id = binding.rgGender.CheckedRadioButtonId;
                    if (id != c_id)
                    {
                        binding.rgGender.Check(id);
                    }
                    SetGenderImageResource(value);
                }
            }
        }

        void SetGenderImageResource(Gender value)
        {
            binding!.ivIconGender.SetImageResource(value switch
            {
                Gender.Male => Resource.Drawable.baseline_male_black_24,
                Gender.Female => Resource.Drawable.baseline_female_black_24,
                _ => Resource.Drawable.baseline_visibility_off_black_24,
            });
        }

        private void RgGender_CheckedChange(object sender, RadioGroup.CheckedChangeEventArgs e)
        {
            if (e == null || binding == null || ViewModel == null) return;
            Gender value;
            if (e.CheckedId == Resource.Id.rbGender0)
            {
                value = Gender.Unknown;
            }
            else if (e.CheckedId == Resource.Id.rbGender1)
            {
                value = Gender.Male;
            }
            else if (e.CheckedId == Resource.Id.rbGender2)
            {
                value = Gender.Female;
            }
            else
            {
                return;
            }
            if (ViewModel.Gender != value)
            {
                ViewModel.Gender = value;
                SetGenderImageResource(value);
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
