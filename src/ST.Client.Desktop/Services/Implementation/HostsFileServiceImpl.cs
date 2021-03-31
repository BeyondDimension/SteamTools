using System.Application.UI.Resx;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Text;
using static System.Application.Services.IHostsFileService;

namespace System.Application.Services.Implementation
{
    internal sealed class HostsFileServiceImpl : IHostsFileService
    {
        #region Mark

        const string MarkStart = "# Steam++ Start";
        const string MarkEnd = "# Steam++ End";
        const string BackupMarkStart = "# Steam++ Backup Start";
        const string BackupMarkEnd = "# Steam++ Backup End";

        static string? GetMarkValue(string[] array)
        {
            if (array.Length == 3 || array.Length == 4)
            {
                var value = string.Join(' ', array);
                if (array.Length == 3)
                {
                    if (string.Equals(value, MarkStart, StringComparison.OrdinalIgnoreCase))
                    {
                        return MarkStart;
                    }
                    if (string.Equals(value, MarkEnd, StringComparison.OrdinalIgnoreCase))
                    {
                        return MarkEnd;
                    }
                }
                else /*if (array.Length == 4)*/
                {
                    if (string.Equals(value, BackupMarkStart, StringComparison.OrdinalIgnoreCase))
                    {
                        return BackupMarkStart;
                    }
                    if (string.Equals(value, BackupMarkEnd, StringComparison.OrdinalIgnoreCase))
                    {
                        return BackupMarkEnd;
                    }
                }
            }
            return default;
        }

        #endregion

        readonly IDesktopPlatformService s;

        public HostsFileServiceImpl(IDesktopPlatformService s)
        {
            this.s = s;
        }

        const long MaxFileLength = 52428800;
        bool TryOperation([NotNullWhen(false)] out string? message,
            out FileInfo fileInfo,
            out bool removeReadOnly,
            bool checkReadOnly = false,
            bool checkMaxLength = true)
        {
            removeReadOnly = false;
            fileInfo = new FileInfo(s.HostsFilePath);
            if (!fileInfo.Exists)
            {
                message = "hosts file was not found";
                return false;
            }
            if (checkMaxLength)
            {
                if (fileInfo.Length > MaxFileLength)
                {
                    message = "hosts file is too large";
                    return false;
                }
            }
            if (checkReadOnly)
            {
                var attr = fileInfo.Attributes;
                if (attr.HasFlag(FileAttributes.ReadOnly))
                {
                    fileInfo.Attributes = attr & ~FileAttributes.ReadOnly;
                    removeReadOnly = true;
                }
            }
            message = null;
            return true;
        }

        public void OpenFile() => s.OpenFileByTextReader(s.HostsFilePath);

        static void SetReadOnly(FileInfo fileInfo)
        {
            try
            {
                var attr = fileInfo.Attributes;
                if (!attr.HasFlag(FileAttributes.ReadOnly))
                {
                    attr |= FileAttributes.ReadOnly;
                    fileInfo.Attributes = attr;
                }
            }
            catch
            {
            }
        }

        static bool HandleLine(int index, HashSet<string> list, string line, out string[] array, Action<string[]>? action = null)
        {
            array = line.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            action?.Invoke(array);
            if (array.Length < 2) return false;
            if (array[0].StartsWith('#')) return false;
            if (array.Length > 2 && !array[2].StartsWith('#'))
                throw new Exception($"hosts file line {index} is malformed");
            if (!list.Add(array[1]))
                throw new Exception($"hosts file line {index} duplicate");
            return true;
        }

        static IEnumerable<(string ip, string domain)> ReadHostsAllLines(StreamReader fileReader)
        {
            int index = 0;
            HashSet<string> list = new();
            while (true)
            {
                index++;
                var line = fileReader.ReadLine();
                if (line == null) break;
                if (string.IsNullOrWhiteSpace(line)) continue;
                if (!HandleLine(index, list, line, out var array)) continue;
                yield return (array[0], array[1]);
            }
        }

        public OperationResult<List<(string ip, string domain)>> ReadHostsAllLines()
        {
            var result = new OperationResult<List<(string ip, string domain)>>(OperationResultType.Error, AppResources.Hosts_ReadError);
            if (!TryOperation(out var errmsg, out var fileInfo, out var _))
            {
                result.Message = errmsg;
                return result;
            }
            try
            {
                using var fileReader = fileInfo.OpenText();
                result.AppendData.AddRange(ReadHostsAllLines(fileReader));
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

        static void HandleUpdate(StringBuilder stringBuilder, Dictionary<string, int> markLength, IEnumerable<(string line, string ip, string domain, int position_before, int position_after)>? backup, IEnumerable<(string ip, string domain)> insert)
        {
            var has_backup = backup.Any_Nullable();
            var has_insert = insert.Any();
            if (!has_backup && !has_insert) return;

            if (has_insert)
            {
                if (markLength.ContainsKey(MarkStart))
                {
                    var position = markLength[MarkStart];
                    foreach (var (ip, domain) in Enumerable.Reverse(insert))
                    {
                        stringBuilder.Insert(position, $"{ip} {domain}");
                    }
                }
                else
                {
                    stringBuilder.AppendLine(MarkStart);
                    markLength.Add(MarkStart, stringBuilder.Length - 1);
                    foreach (var (ip, domain) in insert)
                    {
                        stringBuilder.AppendFormat("{0} {1}", ip, domain);
                        stringBuilder.AppendLine();
                    }
                    stringBuilder.AppendLine(MarkEnd);
                    markLength.Add(MarkEnd, stringBuilder.Length - 1);
                }
            }

            if (has_backup)
            {
                if (markLength.ContainsKey(BackupMarkStart))
                {
                    var position = markLength[BackupMarkStart];
                    foreach (var (line, _, _, _, _) in backup)
                    {
                        stringBuilder.Insert(position, line);
                    }
                }
                else
                {
                    stringBuilder.AppendLine(BackupMarkStart);
                    markLength.Add(BackupMarkStart, stringBuilder.Length - 1);
                    foreach (var (line, _, _, _, _) in backup)
                    {
                        stringBuilder.AppendLine(line);
                    }
                    stringBuilder.AppendLine(BackupMarkEnd);
                    markLength.Add(BackupMarkEnd, stringBuilder.Length - 1);
                }
                foreach (var (_, _, _, position_before, position_after) in backup)
                {
                    stringBuilder.Remove(position_before, position_after - position_before);
                }

                var insert2 = backup.Select(x => (x.ip, x.domain));
                HandleUpdate(stringBuilder, markLength, null, insert2);
            }
        }

        public OperationResult UpdateHosts(string ip, string domain)
        {
            var dict = new Dictionary<string, string>
            {
                { domain, ip },
            };
            return UpdateHosts(dict);
        }

        public OperationResult UpdateHosts(IEnumerable<(string ip, string domain)> hosts)
        {
            var value = hosts.ToDictionary(k => k.domain, v => v.ip);
            return UpdateHosts(value);
        }

        public OperationResult UpdateHosts(IReadOnlyDictionary<string, string> hosts)
        {
            var result = new OperationResult(OperationResultType.Error, AppResources.Hosts_WirteError);
            if (!TryOperation(out var errmsg, out var fileInfo, out var removeReadOnly, checkReadOnly: true))
            {
                result.Message = errmsg;
                return result;
            }
            try
            {
                StringBuilder stringBuilder = new();
                Dictionary<string, int> markLength = new();
                List<(string line, string ip, string domain, int position_before, int position_after)> backup = new();
                List<(string ip, string domain)> insert = new();
                using (var fileReader = fileInfo.OpenText())
                {
                    int index = 0;
                    HashSet<string> list = new();
                    while (true)
                    {
                        index++;
                        var position_before = stringBuilder.Length - 1;
                        var line = fileReader.ReadLine();
                        stringBuilder.AppendLine(line);
                        var position_after = stringBuilder.Length - 1;
                        if (line == null) break;
                        if (string.IsNullOrWhiteSpace(line)) continue;
                        if (!HandleLine(index, list, line, out var array, arr =>
                        {
                            var mark = GetMarkValue(arr);
                            if (mark == null) return;
                            if (markLength.ContainsKey(mark)) throw new Exception($"hosts file mark duplicate, value: {mark}");
                            markLength.Add(mark, stringBuilder.Length - 1);
                        })) continue;
                        var domain = array[1];
                        if (hosts.ContainsKey(domain)) // 发现域名相同项
                        {
                            var ip = hosts[domain];
                            bool inMarkArea; // 在标记区域内
                            if (markLength.ContainsKey(MarkStart))
                            {
                                inMarkArea = !markLength.ContainsKey(MarkEnd);
                            }
                            else
                            {
                                inMarkArea = false;
                            }

                            if (inMarkArea) // 如果在 mark 区域内，直接覆盖
                            {
                                stringBuilder.Remove(position_before, position_after - position_before);
                                stringBuilder.AppendFormat("{0} {1}", ip, domain);
                                stringBuilder.AppendLine();
                            }
                            else // 否则则先写入 bak 区域内，再删除，再写入
                            {
                                backup.Add((line, ip, domain, position_before, position_after));
                            }
                        }
                    }
                    list.ExceptWith(hosts.Keys);
                    if (list.Any())
                    {
                        insert.AddRange(hosts.Where(x => list.Contains(x.Key)).Select(x => (ip: x.Value, domain: x.Key)));
                    }
                }
                HandleUpdate(stringBuilder, markLength, backup, insert);

                var contents = stringBuilder.ToString();
                File.WriteAllText(fileInfo.FullName, contents);

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

            if (removeReadOnly) SetReadOnly(fileInfo);
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
                            return !temp.Contains(Constants.CERTIFICATE_TAG);
                        }
                    }
                    return true;
                });

                var start = dataLines.IndexOf(Constants.CERTIFICATE_TAG + " Start");
                var end = dataLines.IndexOf(Constants.CERTIFICATE_TAG + " End");
                if (start >= 0 && end >= 0)
                    for (var i = start; i <= end; i++)
                        dataLines.RemoveAt(i);

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