namespace System.Net.Http
{
    partial class GeneralHttpClientFactory
    {
        /// <summary>
        /// 默认超时时间，45 秒
        /// </summary>
        public static readonly TimeSpan DefaultTimeout = TimeSpan.FromMilliseconds(DefaultTimeoutMilliseconds);

        /// <inheritdoc cref="DefaultTimeout"/>
        public const int DefaultTimeoutMilliseconds = 45000;
    }
}