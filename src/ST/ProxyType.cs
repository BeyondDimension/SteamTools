namespace System.Application
{
    /// <summary>
    /// 代理类型
    /// </summary>
    public enum ProxyType
    {
        /// <summary>
        /// 启用重定向
        /// </summary>
        Redirect = 1,

        /// <summary>
        /// 本地代理
        /// </summary>
        Local,

        /// <summary>
        /// 直接成功
        /// </summary>
        DirectSuccess,

        /// <summary>
        /// 直接失败
        /// </summary>
        DirectFailure,

        /// <summary>
        /// 服务器加速
        /// </summary>
        ServerAccelerate,
    }
}