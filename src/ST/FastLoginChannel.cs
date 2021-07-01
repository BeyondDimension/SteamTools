namespace System.Application
{
    /// <summary>
    /// 快速登录渠道
    /// <list type="bullet">
    /// <item>AuthenticationController.ExternalLogin/ExternalLoginCallback</item>
    /// <item>UserManager.LoginSharedAsync</item>
    /// <item>UserManager.CreateAccountAsync</item>
    /// <item>UserManager.BindAccountAsync</item>
    /// <item>UserManager.FindByXAccountIdAsync</item>
    /// <item>UserManager.UnbundleAccountAsync</item>
    /// <item>IAccountDeleteRecordDbContext.DbSet_XUserTokens</item>
    /// <item>ApplicationDbContext.DbSet_XUserTokens/X_Accounts</item>
    /// <item>AccountDeleteRecord</item>
    /// <item>AccountDeleteRecordRepository.DeleteAccount</item>
    /// </list>
    /// </summary>
    public enum FastLoginChannel
    {
        /// <summary>
        /// <list type="bullet">
        /// <item>Documentation：https://steamcommunity.com/dev </item>
        /// <item>Package：https://www.nuget.org/packages/AspNet.Security.OpenId.Steam </item>
        /// </list>
        /// </summary>
        Steam,

        /// <summary>
        /// <list type="bullet">
        /// <item>Documentation：https://docs.microsoft.com/zh-cn/aspnet/core/security/authentication/social/microsoft-logins </item>
        /// <item>Package：https://www.nuget.org/packages/Microsoft.AspNetCore.Authentication.MicrosoftAccount </item>
        /// </list>
        /// </summary>
        Microsoft,

        /// <summary>
        /// <list type="bullet">
        /// <item>Documentation：https://developers.e.qq.com/docs/apilist/auth/oauth2 </item>
        /// <item>Package：https://www.nuget.org/packages/AspNet.Security.OAuth.QQ </item>
        /// </list>
        /// </summary>
        QQ,

        /// <summary>
        /// <list type="bullet">
        /// <item>Documentation：https://developer.apple.com/documentation/signinwithapplerestapi </item>
        /// <item>Package：https://www.nuget.org/packages/AspNet.Security.OAuth.Apple </item>
        /// </list>
        /// </summary>
        Apple,


        Xbox = Microsoft,
    }
}