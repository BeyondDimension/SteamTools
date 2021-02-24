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

        OperationResult<List<string>> ReadHostsAllLines();

        OperationResult UpdateHosts(string ip, string domain);

        OperationResult UpdateHosts(IReadOnlyList<(string ip, string domain)> hosts);

        OperationResult RemoveHosts(string ip, string domain);

        OperationResult RemoveHostsByTag();
    }

#if DEBUG

    [Obsolete("use IHostsFileService.Instance", true)]
    public class HostsService
    {
        [Obsolete("use IHostsFileService.Instance.OpenFile", true)]
        public void StartNotepadEditHosts() => throw new NotImplementedException();
    }

#endif
}