namespace System.Net.Http
{
    partial class GeneralHttpClientFactory
    {
        /// <summary>
        /// 默认超时时间，45 秒
        /// </summary>
        public static readonly TimeSpan DefaultTimeout = TimeSpan.FromSeconds(45);

        static readonly Lazy<int> mDefaultTimeoutTotalMilliseconds = new(() => DefaultTimeout.TotalMilliseconds.ToInt32());

        /// <inheritdoc cref="DefaultTimeout"/>
        public static int DefaultTimeoutTotalMilliseconds => mDefaultTimeoutTotalMilliseconds.Value;
    }
}