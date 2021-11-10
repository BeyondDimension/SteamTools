using Android.Views;
using Binding;
using System.Application.UI.Resx;
using System.Application.UI.ViewModels;

namespace System.Application.UI.Fragments
{
    internal sealed class UserBasicInfoFragment : BaseFragment<fragment_basic_profile, UserProfileWindowViewModel>
    {
        protected override int? LayoutResource => Resource.Layout.fragment_basic_profile;

        public override void OnCreateView(View view)
        {
            base.OnCreateView(view);

            R.Subscribe(() =>
            {
                if (binding == null) return;
                binding.btnUploadYourAvatar.Text = AppResources.UploadYourAvatar;
                binding.tvUploadYourAvatarDesc.Text = $"{AppResources.UploadYourAvatarDesc}{Environment.NewLine}{AppResources.UnderConstruction}";
                binding.layoutNickName.Hint = AppResources.NickName;
                binding.layoutPhoneNumber.Hint = AppResources.User_Phone;
                binding.btnModifyPhoneNumber.Text = AppResources.Modify;
                binding.layoutArea2.Hint = AppResources.UserProfile_Location;
                binding.layoutGender.Hint = AppResources.Gender;
                binding.layoutBirthDate.Hint = AppResources.UserProfile_BirthDate;
                binding.btnSubmit.Text = AppResources.SaveChanges;
            }).AddTo(this);
        }
    }
}
