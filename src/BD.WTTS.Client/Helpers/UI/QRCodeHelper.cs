// ReSharper disable once CheckNamespace
namespace BD.WTTS;

public static partial class QRCodeHelper
{
    /// <summary>
    /// QRCode 生成结果
    /// </summary>
    public enum QRCodeCreateResult
    {
        Success,
        DataTooLong,
        Exception,
    }
}