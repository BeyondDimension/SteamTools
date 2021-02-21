namespace System.Application
{
    /// <summary>
    /// 短信验证码类型
    /// </summary>
    public enum SmsCodeType
    {
        /// <summary>
        /// [通过手机号码注册账号]用于注册
        /// </summary>
        [Obsolete("register has been deprecated")]
        Register = 200,

        /// <summary>
        /// [通过手机号码重设密码]找回密码
        /// </summary>
        [Obsolete("password has been deprecated")]
        ForgotPassword = 201,

        /// <summary>
        /// [换新手机号要旧手机号短信验证]绑定新手机号
        /// </summary>
        ChangePhoneNumberNew = 202,

        /// <summary>
        /// [新手机号的短信验证]换绑手机（安全验证）
        /// </summary>
        ChangePhoneNumberValidation = 203,

        /// <summary>
        /// [通过手机号码]登录或注册
        /// </summary>
        LoginOrRegister = 204,
    }
}