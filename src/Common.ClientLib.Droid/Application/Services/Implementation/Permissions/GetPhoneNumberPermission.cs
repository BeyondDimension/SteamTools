using Android;
using Android.OS;
using XEPermissions = Xamarin.Essentials.Permissions;

namespace System.Application.Services.Implementation.Permissions
{
    internal sealed class GetPhoneNumberPermission : XEPermissions.BasePlatformPermission, IPermissions.IGetPhoneNumber
    {
        XEPermissions.BasePermission IPermissions.IPermission.Permission => this;

        public const string Description = "自动填充输入框中的手机号码";

        public override (string androidPermission, bool isRuntime)[] RequiredPermissions
        {
            get
            {
                string androidPermission;
                if (Build.VERSION.SdkInt >= BuildVersionCodes.O)
                {
                    androidPermission = Manifest.Permission.ReadPhoneNumbers;
                }
                else
                {
                    androidPermission = Manifest.Permission.ReadPhoneState;
                }
                return new[] { (androidPermission, true) };
            }
        }
    }
}