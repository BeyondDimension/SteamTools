using SteamTool.Core.Common;
using SteamTool.Core.Properties;
using SteamTool.Model;
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
        public readonly static string HostsPath = @$"{Environment.SystemDirectory}\drivers\etc\hosts";

        static HostsService()
        {
            if (!File.Exists(HostsPath))
            {
                using var fs = File.Create(HostsPath);
                fs.Close();
            }
        }

        public OperationResult<List<string>> ReadHostsAllLines()
        {
            var result = new OperationResult<List<string>>(OperationResultType.Error, Resources.Hosts_ReadError);
            if (!File.Exists(HostsPath))
            {
                result.Message = "hosts file was not found";
                return result;
            }
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
            if (!File.Exists(HostsPath))
            {
                result.Message = "hosts file was not found";
                return result;
            }
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
                dataLines.Add($"{ip} {domain} {Const.HOST_TAG}");
                File.WriteAllLines(HostsPath, dataLines);

                //File.SetAttributes(HostsPath, FileAttributes.ReadOnly);
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
            if (!File.Exists(HostsPath))
            {
                result.Message = "hosts file was not found";
                return result;
            }
            if (hosts.Count == 0)
            {
                return result;
            }

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
                        foreach (var (ip, domain) in hosts)
                        {
                            if (temp.Contains(domain))
                            {
                                //相同域名直接修改删除
                                return false;
                            }
                        }
                    }
                }
                return true;
            });

            foreach (var (ip, domain) in hosts)
            {
                dataLines.Add($"{ip} {domain} {Const.HOST_TAG}");
            }
            try
            {
                File.WriteAllLines(HostsPath, dataLines);
                //File.SetAttributes(HostsPath, FileAttributes.ReadOnly);
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
            if (!File.Exists(HostsPath))
            {
                result.Message = "hosts file was not found";
                return result;
            }
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

                //File.SetAttributes(HostsPath, FileAttributes.ReadOnly);
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
            if (!File.Exists(HostsPath))
            {
                result.Message = "hosts file was not found";
                return result;
            }
            //操作前取消只读属性
            File.SetAttributes(HostsPath, FileAttributes.Normal);

            var dataLines = File.ReadAllLines(HostsPath, Encoding.Default).ToList().FindAll(s =>
            {
                var temp = s.Trim().Split(' ').ToList().FindAll(w => !string.IsNullOrEmpty(w));
                //有效数据一行内至少要有两列数据
                if (temp.Count >= 2)
                {
                    if (!temp[0].StartsWith("#"))
                    {
                        return !temp.Contains(Const.HOST_TAG);
                    }
                }
                return true;
            });

            try
            {
                File.WriteAllLines(HostsPath, dataLines);
                //File.SetAttributes(HostsPath, FileAttributes.ReadOnly);
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


        public void StartNotepadEditHosts()
        {
            System.Diagnostics.Process.Start("notepad.exe", HostsPath);
        }
    }
}
