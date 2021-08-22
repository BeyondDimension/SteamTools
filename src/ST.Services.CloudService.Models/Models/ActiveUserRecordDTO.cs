using System.Runtime.InteropServices;
using MPKey = MessagePack.KeyAttribute;
using MPObj = MessagePack.MessagePackObjectAttribute;
using N_JsonProperty = Newtonsoft.Json.JsonPropertyAttribute;
using S_JsonProperty = System.Text.Json.Serialization.JsonPropertyNameAttribute;

namespace System.Application.Models
{
    /// <summary>
    /// (匿名)活跃用户纪录，用于统计分析以改进体验，详情见使用协议声明
    /// </summary>
    [MPObj]
    public class ActiveUserRecordDTO : IExplicitHasValue
    {
        [MPKey(0)]
        [N_JsonProperty("0")]
        [S_JsonProperty("0")]
        public ActiveUserType Type { get; set; }

        /// <summary>
        /// 使用平台
        /// </summary>
        [MPKey(1)]
        [N_JsonProperty("1")]
        [S_JsonProperty("1")]
        public Platform Platform { get; set; } = DeviceInfo2.Platform;

        /// <summary>
        /// 设备类型
        /// </summary>
        [MPKey(2)]
        [N_JsonProperty("2")]
        [S_JsonProperty("2")]
        public DeviceIdiom DeviceIdiom { get; set; } = DeviceInfo2.Idiom;

        /// <summary>
        /// 系统版本号
        /// </summary>
        [MPKey(3)]
        [N_JsonProperty("3")]
        [S_JsonProperty("3")]
        public string? OSVersion { get; set; } = Environment.OSVersion.Version.ToString();

        /// <summary>
        /// 屏幕总数
        /// </summary>
        [MPKey(4)]
        [N_JsonProperty("4")]
        [S_JsonProperty("4")]
        public int ScreenCount { get; set; }

        /// <summary>
        /// 主屏幕像素密度
        /// </summary>
        [MPKey(5)]
        [N_JsonProperty("5")]
        [S_JsonProperty("5")]
        public double PrimaryScreenPixelDensity { get; set; }

        /// <summary>
        /// 主屏幕宽度
        /// </summary>
        [MPKey(6)]
        [N_JsonProperty("6")]
        [S_JsonProperty("6")]
        public int PrimaryScreenWidth { get; set; }

        /// <summary>
        /// 主屏幕高度
        /// </summary>
        [MPKey(7)]
        [N_JsonProperty("7")]
        [S_JsonProperty("7")]
        public int PrimaryScreenHeight { get; set; }

        /// <summary>
        /// 总屏幕宽度
        /// </summary>
        [MPKey(8)]
        [N_JsonProperty("8")]
        [S_JsonProperty("8")]
        public int SumScreenWidth { get; set; }

        /// <summary>
        /// 总屏幕高度
        /// </summary>
        [MPKey(9)]
        [N_JsonProperty("9")]
        [S_JsonProperty("9")]
        public int SumScreenHeight { get; set; }

        /// <summary>
        /// 当前进程 CPU 构架
        /// </summary>
        [MPKey(10)]
        [N_JsonProperty("10")]
        [S_JsonProperty("10")]
        public ArchitectureFlags ProcessArch { get; set; } = RuntimeInformation.ProcessArchitecture.Convert(false);

        bool IExplicitHasValue.ExplicitHasValue()
        {
            return Type.IsDefined() && Platform.IsDefined() && DeviceIdiom.IsDefined() && !string.IsNullOrWhiteSpace(OSVersion) && ProcessArch.IsDefined();
        }
    }
}