namespace System.Application.UI
{
    public static partial class QRCodeHelper
    {
        /// <summary>
        /// QRCode生成结果
        /// </summary>
        public enum QRCodeCreateResult
        {
            Success,
            DataTooLong,
            Exception,
        }
    }
}