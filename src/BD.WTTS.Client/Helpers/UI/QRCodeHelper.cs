// ReSharper disable once CheckNamespace
namespace BD.WTTS;

[Mobius(
"""
Mobius.Helpers.QRCodeHelper
""")]
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