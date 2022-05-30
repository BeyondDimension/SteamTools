using Android;
using Android.OS;

namespace System.Application.Services.Implementation.Permissions;

internal sealed class GetPhoneNumberPermission : BasePlatformPermission, Permissions2.IGetPhoneNumber
{
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