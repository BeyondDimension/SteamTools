using System.Collections.Generic;

namespace System.Application.Services
{
    /// <summary>
    /// hosts 文件助手服务
    /// </summary>
    public interface IHostsFileService
    {
        const string TAG = "HostsFileS";

        public static IHostsFileService Instance => DI.Get<IHostsFileService>();

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

        static bool mOnExitRestoreHosts;
        static readonly object mOnExitRestoreHostsLock = new();

        /// <summary>
        /// 当程序退出时还原 hosts 文件
        /// </summary>
        static void OnExitRestoreHosts()
        {
            lock (mOnExitRestoreHostsLock)
            {
                if (mOnExitRestoreHosts) return;
                Instance.RemoveHostsByTag();
                mOnExitRestoreHosts = true;
            }
        }

        bool ContainsHostsByTag();
    }
}