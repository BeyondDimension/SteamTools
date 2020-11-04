using SteamTool.Core.Common;
using SteamTool.Core.Properties;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace SteamTool.Core
{
    public class HostsService
    {
        public readonly static string HostsPath = $@"{Environment.GetFolderPath(Environment.SpecialFolder.System)}\drivers\etc\hosts";

        public string HostTag { get; set; } = $"#S302";

        public OperationResult<List<string>> ReadHostsAllLines()
        {
            var result = new OperationResult<List<string>>(OperationResultType.Error, Resources.Hosts_ReadError);
            try
            {
                var dataLines = File.ReadAllLines(HostsPath).ToList();
                foreach (var line in dataLines)
                {
                    var temp = line.Trim().Split(' ').ToList();
                    temp = temp.FindAll(s => !string.IsNullOrEmpty(s));
                    if (temp.Count >= 2)
                    {
                        if (!temp[0].StartsWith("#"))
                        {
                            result.AppendData.Add(temp.ToString());
                        }
                    }
                }
                result.ResultType = OperationResultType.Success;
                result.Message = Resources.Hosts_ReadSuccess;
            }
            catch (Exception ex)
            {
                result.ResultType = OperationResultType.Error;
                result.Message = ex.Message;
            }
            return result;
        }

        public OperationResult UpdateHosts(string ip, string domain)
        {
            var result = new OperationResult(OperationResultType.Error, Resources.Hosts_WirteError);

            //操作前取消只读属性
            File.SetAttributes(HostsPath, FileAttributes.Normal);

            //避免重复写入
            var dataLines = File.ReadAllLines(HostsPath, Encoding.Default).ToList().FindAll(s =>
            {
                var temp = s.Trim().Split(' ').ToList();
                temp = temp.FindAll(s => !string.IsNullOrEmpty(s));
                //一行内至少要有两列数据
                if (temp.Count >= 2)
                {
                    if (!temp[0].StartsWith("#"))
                    {
                        if (temp.Contains(domain))
                        {
                            //相同域名直接修改删除
                            return false;
                        }
                    }
                }
                return true;
            });

            try
            {
                dataLines.Add($"{ip} {domain} {HostTag}");
                File.WriteAllLines(HostsPath, dataLines);

                File.SetAttributes(HostsPath, FileAttributes.ReadOnly);
                result.ResultType = OperationResultType.Success;
                result.Message = Resources.Hosts_UpdateSuccess;
            }
            catch (Exception ex)
            {
                result.ResultType = OperationResultType.Error;
                result.AppendData = ex;
                result.Message = ex.Message;
                return result;
            }
            return result;
        }

        public OperationResult UpdateHosts(List<(string ip, string domain)> hosts)
        {
            var result = new OperationResult(OperationResultType.Error, Resources.Hosts_WirteError);

            //操作前取消只读属性
            File.SetAttributes(HostsPath, FileAttributes.Normal);

            //避免重复写入
            var dataLines = File.ReadAllLines(HostsPath, Encoding.Default).ToList().FindAll(s =>
            {
                var temp = s.Trim().Split(' ').ToList();
                temp = temp.FindAll(s => !string.IsNullOrEmpty(s));
                //一行内至少要有两列数据
                if (temp.Count >= 2)
                {
                    if (!temp[0].StartsWith("#"))
                    {
                        foreach (var item in hosts)
                        {
                            if (temp.Contains(item.domain))
                            {
                                //相同域名直接修改删除
                                return false;
                            }
                        }
                    }
                }
                return true;
            });

            foreach (var item in hosts)
            {
                dataLines.Add($"{item.ip} {item.domain} {HostTag}");
            }
            try
            {
                File.WriteAllLines(HostsPath, dataLines);
                File.SetAttributes(HostsPath, FileAttributes.ReadOnly);
                result.ResultType = OperationResultType.Success;
                result.Message = Resources.Hosts_UpdateSuccess;
            }
            catch (Exception ex)
            {
                result.ResultType = OperationResultType.Error;
                result.AppendData = ex;
                result.Message = ex.Message;
                return result;
            }
            return result;
        }

        public OperationResult RemoveHosts(string ip, string domain)
        {
            var result = new OperationResult(OperationResultType.Error, Resources.Hosts_WirteError);

            //操作前取消只读属性
            File.SetAttributes(HostsPath, FileAttributes.Normal);

            //避免重复写入
            var dataLines = File.ReadAllLines(HostsPath, Encoding.Default).ToList().FindAll(s =>
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

            try
            {
                File.WriteAllLines(HostsPath, dataLines);

                File.SetAttributes(HostsPath, FileAttributes.ReadOnly);
                result.ResultType = OperationResultType.Success;
                result.Message = Resources.Hosts_UpdateSuccess;
            }
            catch (Exception ex)
            {
                result.ResultType = OperationResultType.Error;
                result.AppendData = ex;
                result.Message = ex.Message;
                return result;
            }
            return result;
        }

        public OperationResult RemoveHostsByTag()
        {
            var result = new OperationResult(OperationResultType.Error, Resources.Hosts_WirteError);

            //操作前取消只读属性
            File.SetAttributes(HostsPath, FileAttributes.Normal);

            //避免重复写入
            var dataLines = File.ReadAllLines(HostsPath, Encoding.Default).ToList().FindAll(s =>
            {
                var temp = s.Trim().Split(' ').ToList();
                temp = temp.FindAll(s => !string.IsNullOrEmpty(s));
                //一行内至少要有两列数据
                if (temp.Count >= 2)
                {
                    if (!temp[0].StartsWith("#"))
                    {
                        return !temp.Contains(HostTag);
                    }
                }
                return true;
            });

            try
            {
                File.WriteAllLines(HostsPath, dataLines);
                File.SetAttributes(HostsPath, FileAttributes.ReadOnly);
                result.ResultType = OperationResultType.Success;
                result.Message = Resources.Hosts_UpdateSuccess;
            }
            catch (Exception ex)
            {
                result.ResultType = OperationResultType.Error;
                result.AppendData = ex;
                result.Message = ex.Message;
                return result;
            }
            return result;
        }
    }
}
