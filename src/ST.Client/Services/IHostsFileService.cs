using System.Collections.Generic;

namespace System.Application.Services
{
    /// <summary>
    /// hosts 文件助手服务
    /// </summary>
    public interface IHostsFileService
    {
        static IHostsFileService Instance => DI.Get<IHostsFileService>();

        protected const string TAG = "HostsFileS";

        /// <summary>
        /// 打开 hosts 文件
        /// </summary>
        void OpenFile();

        /// <summary>
        /// 读取 hosts 文件
        /// </summary>
        /// <returns></returns>
        OperationResult<List<(string ip, string domain)>> ReadHostsAllLines();

        /// <summary>
        /// 更新一条 hosts 纪录
        /// </summary>
        /// <param name="ip"></param>
        /// <param name="domain"></param>
        /// <returns></returns>
        OperationResult UpdateHosts(string ip, string domain);

        /// <inheritdoc cref="UpdateHosts(IReadOnlyDictionary{string, string})"/>\
        OperationResult UpdateHosts(IEnumerable<(string ip, string domain)> hosts);

        /// <summary>
        /// 更新多条 hosts 纪录
        /// </summary>
        /// <param name="hosts"></param>
        /// <returns></returns>
        OperationResult UpdateHosts(IReadOnlyDictionary<string, string> hosts);

        /// <summary>
        /// 移除一条 hosts 纪录
        /// </summary>
        /// <param name="ip"></param>
        /// <param name="domain"></param>
        /// <returns></returns>
        [Obsolete]
        OperationResult RemoveHosts(string ip, string domain);

        /// <inheritdoc cref="RemoveHosts(string, string)"/>
        OperationResult RemoveHosts(string domain);

        /// <summary>
        /// 移除当前程序写入的 hosts 纪录并还原写入时冲突的备份纪录
        /// </summary>
        /// <returns></returns>
        OperationResult RemoveHostsByTag();

        /// <summary>
        /// 当程序退出时还原 hosts 文件
        /// </summary>
        void OnExitRestoreHosts();

        bool ContainsHostsByTag();

        enum EncodingType : byte
        {
            /// <summary>
            /// 自动，在 Windows 上使用 <see cref="EncodingType.UTF8WithBOM"/>，在其他操作系统上使用 <see cref="UTF8"/>
            /// </summary>
            Auto,

            /// <summary>
            /// (仅 Windows)系统的活动代码页并创建 Encoding 与其对应的对象。 
            /// 活动代码页可能是 ANSI 代码页，其中包括 ASCII 字符集以及不同于代码页的其他字符。
            /// <para></para>
            /// 在非 Windows 上此项与 <see cref="UTF8"/> 行为一致。
            /// </summary>
            ANSICodePage,

            UTF8,

            UTF8WithBOM,
        }
    }
}