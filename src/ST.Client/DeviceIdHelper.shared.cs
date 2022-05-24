#pragma warning disable IDE0079
#pragma warning disable IDE0005
using System.Security.Cryptography;
#pragma warning restore IDE0005
#pragma warning restore IDE0079

namespace System.Application;

/// <summary>
/// 用于匿名统计的设备 Id 生成助手类
/// </summary>
static partial class DeviceIdHelper
{
    const int DeviceIdRLength = 7;

    public const int MaxLength = ShortGuid.StringLength + DeviceIdRLength + Hashs.String.Lengths.SHA256;
}
