using Android.App;
using static Android.Manifest.Permission;

#region Base
// 允许应用程序访问互联网。
[assembly: UsesPermission(Internet)] // android.permission.INTERNET
#endregion

#region Xamarin.Essentials.Connectivity // https://docs.microsoft.com/zh-cn/xamarin/essentials/connectivity?context=xamarin%2Fandroid&tabs=android
// 允许应用程序访问有关网络的信息。
[assembly: UsesPermission(AccessNetworkState)] // android.permission.ACCESS_NETWORK_STATE
#endregion

#region Auto-fill the phone number in the EditText
// 允许只读访问电话状态，包括设备的电话号码，当前蜂窝网络信息，任何正在进行的呼叫的状态以及PhoneAccount在设备上注册的任何列表 。
[assembly: UsesPermission(ReadPhoneState, MaxSdkVersion = 25)] // android.permission.READ_PHONE_STATE
// 允许读取设备的电话号码。
[assembly: UsesPermission(ReadPhoneNumbers)] // android.permission.READ_PHONE_NUMBERS
#endregion

#region Xamarin.Essentials.FilePicker https://docs.microsoft.com/zh-cn/xamarin/essentials/file-picker?context=xamarin%2Fandroid&tabs=android
[assembly: UsesPermission(ReadExternalStorage)]
#endregion