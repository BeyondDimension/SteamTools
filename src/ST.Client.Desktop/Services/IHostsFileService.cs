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

        OperationResult<List<(string ip, string domain)>> ReadHostsAllLines();

        OperationResult UpdateHosts(string ip, string domain);

        [Obsolete]
        OperationResult UpdateHosts(IEnumerable<(string ip, string domain)> hosts);

        OperationResult RemoveHosts(string ip, string domain);

        OperationResult RemoveHostsByTag();
    }
}