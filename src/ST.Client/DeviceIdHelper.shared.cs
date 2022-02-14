using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;

namespace System.Application
{
    /// <summary>
    /// 用于匿名统计的设备 Id 生成助手类
    /// </summary>
    static partial class DeviceIdHelper
    {
        const int DeviceIdRLength = 7;

        public const int MaxLength = ShortGuid.StringLength + DeviceIdRLength + Hashs.String.Lengths.SHA256;
    }
}
