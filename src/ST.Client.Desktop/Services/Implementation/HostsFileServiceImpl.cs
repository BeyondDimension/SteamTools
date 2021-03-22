using System.Application.UI.Resx;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using static System.Application.Services.IHostsFileService;

namespace System.Application.Services.Implementation
{
    internal sealed class HostsFileServiceImpl : IHostsFileService
    {
        readonly IDesktopPlatformService s;

        public HostsFileServiceImpl(IDesktopPlatformService s)
        {
            this.s = s;
        }

        public void OpenFile() => s.OpenFileByTextReader(s.HostsFilePath);

        public OperationResult<List<string>> ReadHostsAllLines()
        {
            var result = new OperationResult<List<string>>(OperationResultType.Error, AppResources.Hosts_ReadError);
            if (!File.Exists(s.HostsFilePath))
            {
                result.Message = "hosts file was not found";
                return result;
            }
            try
            {
                var dataLines = File.ReadAllLines(s.HostsFilePath).ToList();
                foreach (var line in dataLines)
                {
                    var temp = line.Trim().Split(' ').ToList();
                    temp = temp.FindAll(s => !string.IsNullOrEmpty(s));
                    if (temp.Count >= 2)
                    {
                        if (!temp[0].StartsWith("#"))
                        {
                            result.AppendData.Add(temp?.ToString() ?? string.Empty);
                        }
                    }
                }
                result.ResultType = OperationResultType.Success;
                result.Message = AppResources.Hosts_ReadSuccess;
            }
            catch (Exception ex)
            {
                Log.Error(TAG, ex, "ReadHostsAllLines catch.");
                result.ResultType = OperationResultType.Error;
                result.Message = ex.Message;
            }
            return result;
        }

        public OperationResult UpdateHosts(string ip, string domain)
        {
            var result = new OperationResult(OperationResultType.Error, AppResources.Hosts_WirteError);
            if (!File.Exists(s.HostsFilePath))
            {
                result.Message = "hosts file was not found";
                return result;
            }
            // 操作前取消只读属性
            File.SetAttributes(s.HostsFilePath, FileAttributes.Normal);

            // 避免重复写入
            var dataLines = File.ReadAllLines(s.HostsFilePath, Encoding.Default).ToList().FindAll(s =>
            {
                var temp = s.Trim().Split(' ').ToList();
                temp = temp.FindAll(s => !string.IsNullOrEmpty(s));
                // 一行内至少要有两列数据
                if (temp.Count >= 2)
                {
                    if (!temp[0].StartsWith("#"))
                    {
                        if (temp.Contains(domain))
                        {
                            // 相同域名直接修改删除
                            return false;
                        }
                    }
                }
                return true;
            });

            try
            {
                dataLines.Add($"{ip} {domain} {Constants.CERTIFICATE_HOST_TAG}");
                File.WriteAllLines(s.HostsFilePath, dataLines, Encoding.UTF8);

                //File.SetAttributes(HostsPath, FileAttributes.ReadOnly);
                result.ResultType = OperationResultType.Success;
                result.Message = AppResources.Hosts_UpdateSuccess;
            }
            catch (Exception ex)
            {
                Log.Error(TAG, ex, "UpdateHosts catch.");
                result.ResultType = OperationResultType.Error;
                result.AppendData = ex;
                result.Message = ex.Message;
                return result;
            }
            return result;
        }

        public OperationResult UpdateHosts(IReadOnlyList<(string ip, string domain)> hosts)
        {
            var result = new OperationResult(OperationResultType.Error, AppResources.Hosts_WirteError);
            if (!File.Exists(s.HostsFilePath))
            {
                result.Message = "hosts file was not found";
                return result;
            }
            if (hosts.Count == 0)
            {
                return result;
            }

            // 操作前取消只读属性
            File.SetAttributes(s.HostsFilePath, FileAttributes.Normal);

            // 避免重复写入
            var dataLines = File.ReadAllLines(s.HostsFilePath, Encoding.Default).ToList().FindAll(s =>
            {
                var temp = s.Trim().Split(' ').ToList();
                temp = temp.FindAll(s => !string.IsNullOrEmpty(s));
                // 一行内至少要有两列数据
                if (temp.Count >= 2)
                {
                    if (!temp[0].StartsWith("#"))
                    {
                        foreach (var (ip, domain) in hosts)
                        {
                            if (temp.Contains(domain))
                            {
                                // 相同域名直接修改删除
                                return false;
                            }
                        }
                    }
                }
                return true;
            });

            foreach (var (ip, domain) in hosts)
            {
                dataLines.Add($"{ip} {domain} {Constants.CERTIFICATE_HOST_TAG}");
            }
            try
            {
                File.WriteAllLines(s.HostsFilePath, dataLines, Encoding.UTF8);
                //File.SetAttributes(HostsPath, FileAttributes.ReadOnly);
                result.ResultType = OperationResultType.Success;
                result.Message = AppResources.Hosts_UpdateSuccess;
            }
            catch (Exception ex)
            {
                Log.Error(TAG, ex, "UpdateHosts catch.");
                result.ResultType = OperationResultType.Error;
                result.AppendData = ex;
                result.Message = ex.Message;
                return result;
            }
            return result;
        }

        public OperationResult RemoveHosts(string ip, string domain)
        {
            var result = new OperationResult(OperationResultType.Error, AppResources.Hosts_WirteError);
            if (!File.Exists(s.HostsFilePath))
            {
                result.Message = "hosts file was not found";
                return result;
            }
            try
            {
                //操作前取消只读属性
                File.SetAttributes(s.HostsFilePath, FileAttributes.Normal);

                //避免重复写入
                var dataLines = File.ReadAllLines(s.HostsFilePath, Encoding.Default).ToList().FindAll(s =>
                {
                    var temp = s.Trim().Split(' ').ToList();
                    temp = temp.FindAll(s => !string.IsNullOrEmpty(s));
                    //一行内至少要有两列数据
                    if (temp.Count >= 2)
                    {
                        if (!temp[0].StartsWith("#"))
                        {
                            return !(temp.Contains(ip) && temp.Contains(domain));
                        }
                    }
                    return true;
                });

                File.WriteAllLines(s.HostsFilePath, dataLines, Encoding.UTF8);

                //File.SetAttributes(HostsPath, FileAttributes.ReadOnly);
                result.ResultType = OperationResultType.Success;
                result.Message = AppResources.Hosts_UpdateSuccess;
            }
            catch (Exception ex)
            {
                Log.Error(TAG, ex, "RemoveHosts catch.");
                result.ResultType = OperationResultType.Error;
                result.AppendData = ex;
                result.Message = ex.Message;
                return result;
            }
            return result;
        }

        public OperationResult RemoveHostsByTag()
        {
            var result = new OperationResult(OperationResultType.Error, AppResources.Hosts_WirteError);
            if (!File.Exists(s.HostsFilePath))
            {
                result.Message = "hosts file was not found";
                return result;
            }
            try
            {
                //操作前取消只读属性
                File.SetAttributes(s.HostsFilePath, FileAttributes.Normal);

                var dataLines = File.ReadAllLines(s.HostsFilePath, Encoding.Default).ToList().FindAll(s =>
                {
                    var temp = s.Trim().Split(' ').ToList().FindAll(w => !string.IsNullOrEmpty(w));
                    //有效数据一行内至少要有两列数据
                    if (temp.Count >= 2)
                    {
                        if (!temp[0].StartsWith("#"))
                        {
                            return !temp.Contains(Constants.CERTIFICATE_HOST_TAG);
                        }
                    }
                    return true;
                });

                File.WriteAllLines(s.HostsFilePath, dataLines, Encoding.UTF8);
                //File.SetAttributes(HostsPath, FileAttributes.ReadOnly);
                result.ResultType = OperationResultType.Success;
                result.Message = AppResources.Hosts_UpdateSuccess;
            }
            catch (Exception ex)
            {
                Log.Error(TAG, ex, "RemoveHostsByTag catch.");
                result.ResultType = OperationResultType.Error;
                result.AppendData = ex;
                result.Message = ex.Message;
                return result;
            }
            return result;
        }
    }
}