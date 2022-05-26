namespace System.Application.Services.Implementation;

/// <inheritdoc cref="ITelephonyService"/>
public abstract class TelephonyServiceImpl : ITelephonyService
{
    readonly IPermissions p;

    public TelephonyServiceImpl(IPermissions p)
    {
        this.p = p;
    }

    /// <summary>
    /// 由特定平台实现的获取手机号码
    /// </summary>
    /// <returns></returns>
    protected abstract string? PlatformGetPhoneNumber();

    /// <summary>
    /// 由特定平台实现的检查权限后获取手机号码
    /// </summary>
    /// <returns></returns>
    protected virtual async Task<string?> PlatformGetPhoneNumberAsync()
    {
        var status = await p.CheckAndRequestAsync(IPermissions.IGetPhoneNumber.Instance);
        if (status.IsOk())
        {
            try
            {
                return PlatformGetPhoneNumber();
            }
            catch
            {
                // Xamarin Exception Stack:
                // Java.Lang.SecurityException: getLine1NumberForDisplay: Neither user 10837 nor current process has android.permission.READ_PHONE_STATE, android.permission.READ_SMS, or android.permission.READ_PHONE_NUMBERS
                //   at Java.Interop.JniEnvironment+InstanceMethods.CallObjectMethod (Java.Interop.JniObjectReference instance, Java.Interop.JniMethodInfo method, Java.Interop.JniArgumentValue* args) [0x0006e] in <1921523bc22e407fa9a0855bdae29335>:0
                //   at Java.Interop.JniPeerMembers+JniInstanceMethods.InvokeVirtualObjectMethod (System.String encodedMember, Java.Interop.IJavaPeerable self, Java.Interop.JniArgumentValue* parameters) [0x0002a] in <1921523bc22e407fa9a0855bdae29335>:0
                //   at Android.Telephony.TelephonyManager.get_Line1Number () [0x0000a] in <1f18ef20dc7d4d4196739d809db4caa6>:0
                //   at System.Application.Services.Implementation.PlatformTelephonyServiceImpl.PlatformGetPhoneNumber () [0x0000f] in <4b916d7fba474bbcb7162327ea0cdf4b>:0
                //   at System.Application.Services.Implementation.TelephonyServiceImpl.PlatformGetPhoneNumberAsync () [0x0007a] in <3072bb714ec4478ab736cfcc1a194a49>:0
                //   at System.Application.Services.Implementation.TelephonyServiceImpl.GetPhoneNumberAsync () [0x00062] in <3072bb714ec4478ab736cfcc1a194a49>:0
                //   at System.Application.Services.ITelephonyService.GetAutoFillPhoneNumberAsync (System.String textBoxText) [0x00062] in <3072bb714ec4478ab736cfcc1a194a49>:0
                //   ------------------------------------------------------------------------------------------------------------------------------------------
                //   at System.Application.UI.Fragments.BindPhoneNumberFragment.OnCreateViewAsync () [0x00082] in <4b916d7fba474bbcb7162327ea0cdf4b>:0
                //   OR
                //   at System.Application.UI.Fragments.ChangePhoneNumberFragment.OnCreateViewAsync () [0x00068] in <4b916d7fba474bbcb7162327ea0cdf4b>:0
                //   ------------------------------------------------------------------------------------------------------------------------------------------
                //   at System.Runtime.CompilerServices.AsyncMethodBuilderCore+<>c.<ThrowAsync>b__7_0 (System.Object state) [0x00000] in <ac5dda0190f24d829c27844a2bccc1dd>:0
                //   at Android.App.SyncContext+<>c__DisplayClass2_0.<Post>b__0 () [0x00000] in <1f18ef20dc7d4d4196739d809db4caa6>:0
                //   at Java.Lang.Thread+RunnableImplementor.Run () [0x00008] in <1f18ef20dc7d4d4196739d809db4caa6>:0
                //   at Java.Lang.IRunnableInvoker.n_Run (System.IntPtr jnienv, System.IntPtr native__this) [0x00008] in <1f18ef20dc7d4d4196739d809db4caa6>:0
                //   at (wrapper dynamic-method) Android.Runtime.DynamicMethodNameCounter.41(intptr,intptr)
                //   at java.lang.SecurityException: getLine1NumberForDisplay: Neither user 10837 nor current process has android.permission.READ_PHONE_STATE, android.permission.READ_SMS, or android.permission.READ_PHONE_NUMBERS
                //   at android.os.Parcel.createExceptionOrNull(Parcel.java:2425)
                //   at android.os.Parcel.createException(Parcel.java:2409)
                //   at android.os.Parcel.readException(Parcel.java:2392)
                //   at android.os.Parcel.readException(Parcel.java:2334)
                //   at com.android.internal.telephony.ITelephony$Stub$Proxy.getLine1NumberForDisplay(ITelephony.java:11405)
                //   at android.telephony.TelephonyManager.getLine1Number(TelephonyManager.java:4755)
                //   at android.telephony.TelephonyManager.getLine1Number(TelephonyManager.java:4723)
                //   at crc643a5e764dec9ff28a.BaseFragment_2.n_onCreateView(Native Method)
                // https://appcenter.ms/orgs/BeyondDimension/apps/Pro-Steam-1/crashes/errors/1522287042u/overview
                // Reports MIUI Android 11 ~ 12
                // https://developer.android.google.cn/about/versions/11/privacy/permissions#phone-numbers
                // class System.Application.Services.Implementation.Permissions.GetPhoneNumberPermission
                // Properties\Permissions.cs
            }
        }
        return null;
    }

    public async Task<string?> GetPhoneNumberAsync()
    {
        var phoneNumber = await PlatformGetPhoneNumberAsync();
        return phoneNumber;
    }
}