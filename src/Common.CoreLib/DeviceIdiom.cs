using System.Collections.Generic;

namespace System
{
    /// <summary>
    /// 设备种类，例如手机，平板，电视，手表
    /// </summary>
    [Flags]
    public enum DeviceIdiom
    {
        /// <summary>
        /// 未知
        /// </summary>
        Unknown = 1,

        /// <summary>
        /// 手机
        /// </summary>
        Phone = 4,

        /// <summary>
        /// 平板
        /// </summary>
        Tablet = 8,

        /// <summary>
        /// 桌面端
        /// </summary>
        Desktop = 16,

        /// <summary>
        /// 电视
        /// </summary>
        TV = 32,

        /// <summary>
        /// 手表
        /// </summary>
        Watch = 64,
    }

    /// <summary>
    /// Enum 扩展 <see cref="DeviceIdiom"/>
    /// </summary>
    public static partial class IdiomEnumExtensions
    {
        /// <summary>
        /// 值是否在定义的范围中，排除 <see cref="DeviceIdiom.Unknown"/>
        /// </summary>
        /// <param name="deviceIdiom"></param>
        /// <returns></returns>
        public static bool IsDefined(this DeviceIdiom deviceIdiom)
        {
            return deviceIdiom != DeviceIdiom.Unknown && Enum.IsDefined(typeof(DeviceIdiom), deviceIdiom);
        }
    }
}