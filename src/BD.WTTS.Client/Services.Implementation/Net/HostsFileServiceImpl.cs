#if (WINDOWS || MACCATALYST || MACOS || LINUX) && !(IOS || ANDROID)
using static BD.WTTS.Services.IHostsFileService;
using AppResources = BD.WTTS.Client.Resources.Strings;
#endif

// ReSharper disable once CheckNamespace
namespace BD.WTTS.Services.Implementation;

internal sealed class HostsFileServiceImpl
#if (WINDOWS || MACCATALYST || MACOS || LINUX) && !(IOS || ANDROID)
    : IHostsFileService
#endif
{
#if (WINDOWS || MACCATALYST || MACOS || LINUX) && !(IOS || ANDROID)

    readonly IPlatformService s;
    readonly object lockObj = new();
    IHostsFileService? _privilegedThis;
#if WINDOWS
    FileSystemWatcher? watcher;
    readonly ConcurrentDictionary<string, string> updateHostsValue = new();

    void StartWatcher()
    {
        if (WindowsPlatformServiceImpl.IsPrivilegedProcess)
        {
            watcher?.Dispose();
            var hostsDirectoryName = Path.GetDirectoryName(s.HostsFilePath);
            var hostsFileName = Path.GetFileName(s.HostsFilePath);
            watcher = new FileSystemWatcher(hostsDirectoryName.ThrowIsNull())
            {
                EnableRaisingEvents = true,
                NotifyFilter = NotifyFilters.Attributes
                              | NotifyFilters.CreationTime
                              | NotifyFilters.DirectoryName
                              | NotifyFilters.FileName
                              | NotifyFilters.LastWrite
                              | NotifyFilters.Size,
                Filter = hostsFileName,
            };
            watcher.Changed += Watcher_Changed;
            watcher.Deleted += Watcher_Deleted;
        }
    }
#endif

    const string TAG = "HostsFileS";

    async ValueTask<IHostsFileService?> GetPrivilegedThisAsync()
    {
#if WINDOWS
        if (Startup.Instance.IsMainProcess && !WindowsPlatformServiceImpl.IsPrivilegedProcess)
        {
            if (_privilegedThis == null)
            {
                var ipc = IPCMainProcessService.Instance;
                _privilegedThis = await ipc.GetServiceAsync<IHostsFileService>(IPlatformService.IPCRoot.moduleName);
            }
            return _privilegedThis;
        }
#endif
        return null;
    }

    public HostsFileServiceImpl(IPlatformService s)
    {
        this.s = s;
    }

#if WINDOWS
    bool isWatcherOverlapHostsing;
    DateTime lastWatcherOverlapHostsTime;
    CancellationTokenSource? lastWatcherOverlapHostsTimeCTS;

    async Task WatcherOverlapHosts()
    {
        if (isWatcherOverlapHostsing || watcher == null)
            return;

        isWatcherOverlapHostsing = true;
        try
        {
            if (!updateHostsValue.IsEmpty)
            {
                if (lastWatcherOverlapHostsTime != default && (DateTime.Now - lastWatcherOverlapHostsTime) < TimeSpan.FromSeconds(2.65d))
                {
                    lastWatcherOverlapHostsTimeCTS?.Cancel();
                    lastWatcherOverlapHostsTimeCTS = new();
                    try
                    {
                        await Task.Delay(Random2.Next(550, 850), lastWatcherOverlapHostsTimeCTS.Token);
                        lastWatcherOverlapHostsTimeCTS = null;
                    }
                    catch (TaskCanceledException)
                    {
                        return;
                    }
                    catch (ObjectDisposedException)
                    {
                        return;
                    }
                }

                lastWatcherOverlapHostsTime = DateTime.Now;
                if (watcher == null)
                    return;
                HandleHosts(isUpdateOrRemove: true, updateHostsValue);
            }
        }
        finally
        {
            isWatcherOverlapHostsing = false;
        }
    }

    async void Watcher_Deleted(object sender, FileSystemEventArgs e)
    {
        await WatcherOverlapHosts();
    }

    async void Watcher_Changed(object sender, FileSystemEventArgs e)
    {
        await WatcherOverlapHosts();
    }
#endif

    #region Mark

    internal const string MarkStart = "# Steam++ Start";
    internal const string MarkEnd = "# Steam++ End";
    internal const string BackupMarkStart = "# Steam++ Backup Start";
    internal const string BackupMarkEnd = "# Steam++ Backup End";

    /// <summary>
    /// 根据行切割数组获取标记值
    /// </summary>
    /// <param name="line_split_array"></param>
    /// <returns></returns>
    static string? GetMarkValue(string[] line_split_array)
    {
        if (line_split_array.Length == 3 || line_split_array.Length == 4)
        {
            var value = string.Join(' ', line_split_array);
            if (line_split_array.Length == 3)
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

    #region FileVerify

    /// <summary>
    /// 最大支持文件大小，50MB
    /// </summary>
    const long MaxFileLength = 52428800;

    /// <summary>
    /// 尝试开始操作前
    /// </summary>
    /// <param name="message"></param>
    /// <param name="fileInfo"></param>
    /// <param name="removeReadOnly"></param>
    /// <param name="checkReadOnly"></param>
    /// <param name="checkMaxLength"></param>
    /// <returns></returns>
    bool TryOperation([NotNullWhen(false)] out string? message,
        [NotNullWhen(true)] out FileInfo? fileInfo,
        out bool removeReadOnly,
        bool checkReadOnly = false,
        bool checkMaxLength = true)
    {
        try
        {
            message = null;
            removeReadOnly = false;
            fileInfo = new FileInfo(s.HostsFilePath);
            if (!fileInfo.Exists)
            {
                try
                {
                    using var stream = fileInfo.Create();
                    s.WriteDefaultHostsContent(stream);
                    return true;
                }
                catch (Exception ex)
                {
                    message = ex.GetAllMessage();
                    return false;
                }
            }
            //if (fileInfo.IsReadOnly&&(OperatingSystem2.IsMacOS() || (OperatingSystem2.IsLinux() && !s.IsAdministrator)))
            //{
            //    var editPath = Path.Combine(IOPath.CacheDirectory, "hosts");
            //    var editInfo = new FileInfo(editPath);
            //    if (editInfo.Exists)
            //        editInfo.Delete();
            //    fileInfo.CopyTo(editPath);
            //    editInfo.Refresh();
            //    fileInfo = editInfo;
            //}
            if (checkMaxLength)
            {
                if (fileInfo.Length > MaxFileLength)
                {
                    message = AppResources.FileSizeTooLarge;
                    return false;
                }
            }
            if (checkReadOnly)
            {
                if (fileInfo.IsReadOnly)
                {
                    try
                    {
                        fileInfo.IsReadOnly = false;
                    }
                    catch
                    {
                        message = (OperatingSystem.IsMacOS() || OperatingSystem.IsLinux()) ?
                            AppResources.FileAttributeIsReadOnlyModifyFailSeeWebPage_.Format(Constants.Urls.OfficialWebsite_UnixHostAccess) :
                            AppResources.FileAttributeIsReadOnlyModifyFailTryBakDelAgain;
                        return false;
                    }
                    removeReadOnly = true;
                }
            }
            return true;
        }
        catch (Exception ex)
        {
            removeReadOnly = false;
            fileInfo = null;
            message = ex.Message;
            return false;
        }
    }

    /// <summary>
    /// 设置文件只读属性
    /// </summary>
    /// <param name="fileInfo"></param>
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

    #endregion

    #region Handle

    static string[] GetLineSplitArray(string line_value) => line_value.Split(' ', StringSplitOptions.RemoveEmptyEntries);

    internal static bool IsV1Format(string[] line_split_array) => line_split_array.Length == 3 && line_split_array[2] == "#Steam++";

    enum HandleLineResult
    {
        /// <summary>
        /// 当前行格式不正确，不处理直接写入
        /// </summary>
        IncorrectFormatNoneHandleWrite_W_False,

        /// <summary>
        /// 当前行格式正确，开始处理
        /// </summary>
        CorrectFormatStartHandle,

        /// <summary>
        /// 当前行格式不正确，不处理也不写入
        /// </summary>
        IncorrectFormatNoneHandleNoWrite_Null,

        /// <summary>
        /// 重复项
        /// </summary>
        Duplicate,
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    static bool? Convert(HandleLineResult handleLineResult)
    {
        return handleLineResult switch
        {
            HandleLineResult.IncorrectFormatNoneHandleWrite_W_False => false,
            HandleLineResult.CorrectFormatStartHandle => true,
            HandleLineResult.IncorrectFormatNoneHandleNoWrite_Null => null,
            _ => throw new ArgumentOutOfRangeException(nameof(handleLineResult), handleLineResult, null),
        };
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    static HandleLineResult HandleLineV2(int line_num, HashSet<string> domains, string line_value, out string[] line_split_array, Func<string[], bool?>? func = null)
    {
        static bool Contains(HashSet<string> hs, string value) => !hs.Add(value);
        return HandleLineV2(Contains, line_num, domains, line_value, out line_split_array, func);
    }

    /// <summary>
    /// 处理一行数据
    /// <para><see langword="false"/> 当前行格式不正确，不处理直接写入</para>
    /// <para><see langword="true"/> 当前行格式正确，开始处理</para>
    /// <para><see langword="null"/> 当前行格式不正确，不处理也不写入</para>
    /// </summary>
    /// <param name="contains"></param>
    /// <param name="line_num"></param>
    /// <param name="domains"></param>
    /// <param name="line_value"></param>
    /// <param name="line_split_array"></param>
    /// <param name="func"></param>
    /// <returns></returns>
    static HandleLineResult HandleLineV2<T>(Func<T, string, bool> contains, int line_num, T domains, string line_value, out string[] line_split_array, Func<string[], bool?>? func = null)
    {
        line_split_array = GetLineSplitArray(line_value);
        if (func != null)
        {
            var r = func.Invoke(line_split_array);
            if (!r.HasValue) return HandleLineResult.IncorrectFormatNoneHandleNoWrite_Null;
            if (!r.Value) return HandleLineResult.IncorrectFormatNoneHandleWrite_W_False;
        }
        if (line_split_array.Length < 2) return HandleLineResult.IncorrectFormatNoneHandleWrite_W_False;
        if (line_split_array[0].StartsWith('#')) return HandleLineResult.IncorrectFormatNoneHandleWrite_W_False;
        if (line_split_array.Length > 2 && !line_split_array[2].StartsWith('#')) return HandleLineResult.IncorrectFormatNoneHandleWrite_W_False;
        if (IsV1Format(line_split_array)) return HandleLineResult.IncorrectFormatNoneHandleNoWrite_Null; // Compat V1
        if (contains != null && contains(domains, line_split_array[1])) return HandleLineResult.Duplicate;
        return HandleLineResult.CorrectFormatStartHandle;
    }

    IOperationResult HandleHosts(bool isUpdateOrRemove, IReadOnlyDictionary<string, string>? hosts = null)
    {
        var result = HandleHosts(isUpdateOrRemove, hosts, out var codeHandleHostsFileException);
        if (codeHandleHostsFileException == HandleHostsFileException.Code_CommunityFix_Hosts_MarkDuplicate_)
        {
            //Toast.Show(ToastIcon.Warning, "");
            File.Copy(s.HostsFilePath, $"{s.HostsFilePath}.spp.bak", true);
#if WINDOWS
            watcher?.Dispose();
            watcher = null;
#endif
            s.WriteDefaultHostsContent();
            result = HandleHosts(isUpdateOrRemove, hosts, out var _);
        }

        return result;
    }

    IOperationResult HandleHosts(bool isUpdateOrRemove, IReadOnlyDictionary<string, string>? hosts, out int codeHandleHostsFileException)
    {
        codeHandleHostsFileException = 0;
        lock (lockObj)
        {
            var result = new OperationResult(OperationResultType.Error, AppResources.Hosts_WirteError);
            if (!TryOperation(out var errmsg, out var fileInfo, out var removeReadOnly, checkReadOnly: true))
            {
                result.Message = errmsg;
                return result;
            }

            try
            {
                var has_hosts = hosts.Any_Nullable();
                if (isUpdateOrRemove && !has_hosts) throw new InvalidOperationException();

                StringBuilder stringBuilder = new();
                HashSet<string> markLength = new(); // mark 标志
                Dictionary<string, string> insert_mark_datas = new(); // 直接插入标记区数据
                Dictionary<string, (int line_num, string line_value)> backup_insert_mark_datas = new(); // 备份插入标记区数据，项已存在的情况下
                Dictionary<string, (int line_num, string line_value)> backup_datas = new(); // 备份区域数据

                var encoding = GetEncoding();
                var result_read = TryReadHosts(() => fileInfo.OpenText(encoding), nameof(HandleHosts));
                if (result_read.ResultType != OperationResultType.Success) return result_read;
                using (var fileReader = result_read.AppendData)
                {
                    fileReader.ThrowIsNull();
                    int line_num = 0;
                    HashSet<string> domains = new(); // 域名唯一检查
                    var last_line_value = (string?)null;
                    while (true)
                    {
                        line_num++;

                        var line_value = fileReader.ReadLine();
                        if (line_value == null) break;

                        var not_append = false;
                        var is_effective_value_v2 = HandleLineV2(line_num, domains, line_value, out var line_split_array, line_split_array =>
                        {
                            if (markLength.Contains(BackupMarkStart) && !markLength.Contains(BackupMarkEnd))
                            {
                                if (line_split_array.Length >= 2 && line_split_array[0].StartsWith('#') && int.TryParse(line_split_array[0].TrimStart('#'), out var bak_line_num)) // #{line_num} {line_value}
                                {
                                    var bak_line_split_array = line_split_array.AsSpan()[1..];
                                    if (bak_line_split_array.Length >= 2)
                                    {
                                        backup_datas.TryAdd(bak_line_split_array[1], (bak_line_num, string.Join(' ', bak_line_split_array.ToArray())));
                                    }
                                }
                                return null;
                            }

                            var mark = GetMarkValue(line_split_array);
                            if (mark == null) return true;
                            if (mark == MarkEnd && !markLength.Contains(MarkStart)) return null;
                            if (mark == BackupMarkEnd && !markLength.Contains(BackupMarkStart)) return null;
                            if ((mark == MarkStart || mark == BackupMarkStart) && last_line_value != null && string.IsNullOrWhiteSpace(last_line_value))
                            {
                                var removeLen = last_line_value.Length + Environment.NewLine.Length;
                                stringBuilder.Remove(stringBuilder.Length - removeLen, removeLen);
                            }
                            if (!markLength.Add(mark))
                            {
                                throw new HandleHostsFileException(AppResources.CommunityFix_Hosts_MarkDuplicate_.Format(mark)) { Code = HandleHostsFileException.Code_CommunityFix_Hosts_MarkDuplicate_ };
                            }
                            return null;
                        });
                        if (is_effective_value_v2 != HandleLineResult.Duplicate)
                        {
                            var is_effective_value = Convert(is_effective_value_v2);
                            if (!is_effective_value.HasValue) goto skip;
                            if (!is_effective_value.Value) goto append; // 当前行是无效值，直接写入
                        }
                        string ip, domain;
                        ip = line_split_array[0];
                        domain = line_split_array[1];
                        var match_domain = has_hosts && hosts!.ContainsKey(domain); // 与要修改的项匹配
                        if (markLength.Contains(MarkStart) && !markLength.Contains(MarkEnd)) // 在标记区域内
                        {
                            if (match_domain)
                            {
                                if (isUpdateOrRemove) // 更新值
                                {
                                    ip = hosts![domain];
                                }
                                else // 删除值
                                {
                                    goto skip;
                                }
                            }
                            insert_mark_datas[domain] = ip;
                            goto skip;
                        }
                        else // 在标记区域外
                        {
                            if (match_domain)
                            {
                                if (isUpdateOrRemove) // 更新值
                                {
                                    insert_mark_datas[domain] = ip;
                                }
                                backup_insert_mark_datas[domain] = (line_num, line_value);
                                goto skip;
                            }
                        }

                        if (not_append)
                        {
                            goto skip;
                        }

                    append: stringBuilder.AppendLine(line_value);
                        last_line_value = line_value;
                        continue;

                    skip: line_num--;
                        continue;
                    }
                }

                if (isUpdateOrRemove)
                {
                    foreach (var item in hosts!)
                    {
                        insert_mark_datas[item.Key] = item.Value;
                    }
                }

                var is_restore = !has_hosts && !isUpdateOrRemove;

                void Restore(IEnumerable<KeyValuePair<string, (int line_num, string line_value)>> datas)
                {
                    if (!is_restore && datas == backup_datas) datas = new List<KeyValuePair<string, (int line_num, string line_value)>>(datas);
                    foreach (var item in datas)
                    {
                        var line_index = stringBuilder.GetLineIndex(item.Value.line_num - 1);
                        var line_value = item.Value.line_value;
                        if (line_index >= 0)
                        {
                            stringBuilder.Insert(line_index, $"{line_value}{Environment.NewLine}");
                        }
                        else
                        {
                            stringBuilder.AppendLine(line_value);
                        }
                        if (!is_restore) backup_datas.Remove(item.Key);
                    }
                }

                if (is_restore)
                {
                    Restore(backup_datas);
                }
                else
                {
                    var any_insert_mark_datas = insert_mark_datas.Any();

                    var restore_backup_datas = any_insert_mark_datas ? backup_datas.Where(x => !insert_mark_datas.ContainsKey(x.Key)) : backup_datas;
                    Restore(restore_backup_datas); // 恢复备份数据

                    if (any_insert_mark_datas) // 插入新增数据
                    {
                        stringBuilder.AppendLine();
                        stringBuilder.AppendLine(MarkStart);
                        foreach (var item in insert_mark_datas)
                        {
                            stringBuilder.AppendFormat("{0} {1}", item.Value, item.Key);
                            stringBuilder.AppendLine();
                        }
                        stringBuilder.AppendLine(MarkEnd);
                    }

                    var any_backup_insert_mark_datas = backup_insert_mark_datas.Any();
                    var insert_or_remove_backup_datas = any_insert_mark_datas ? backup_datas.Where(x => insert_mark_datas.ContainsKey(x.Key)) : backup_datas;
                    if (any_backup_insert_mark_datas) insert_or_remove_backup_datas = insert_or_remove_backup_datas.Where(x => !backup_insert_mark_datas.ContainsKey(x.Key));
                    if (any_backup_insert_mark_datas || insert_or_remove_backup_datas.Any()) // 插入备份数据
                    {
                        stringBuilder.AppendLine();
                        stringBuilder.AppendLine(BackupMarkStart);
                        if (any_backup_insert_mark_datas)
                        {
                            foreach (var item in backup_insert_mark_datas) // #{line_num} {line_value}
                            {
                                stringBuilder.Append('#');
                                stringBuilder.Append(item.Value.line_num);
                                stringBuilder.Append(' ');
                                stringBuilder.AppendLine(item.Value.line_value);
                            }
                        }
                        foreach (var item in insert_or_remove_backup_datas)
                        {
                            stringBuilder.AppendLine(item.Value.line_value);
                        }
                        stringBuilder.AppendLine(BackupMarkEnd);
                    }
                }

                var contents = stringBuilder.ToString();
                File.WriteAllText(fileInfo.FullName, contents, encoding);

                result.ResultType = OperationResultType.Success;
                result.Message = AppResources.Hosts_UpdateSuccess;
            }
            catch (Exception ex)
            {
                Log.Error(TAG, ex, "UpdateHosts catch.");
                result.ResultType = OperationResultType.Error;
                result.AppendData = ex;
                if (ex is HandleHostsFileException hfex)
                {
                    codeHandleHostsFileException = hfex.Code;
                    result.Message = hfex.Message;
                }
                else if (ex is UnauthorizedAccessException || ex is SecurityException)
                {
                    result.Message = AppResources.FileUnauthorized;
                }
                else
                {
                    result.Message = ex.GetAllMessage();
                }
                return result;
            }

            if (removeReadOnly) SetReadOnly(fileInfo);
            return result;
        }
    }

    #endregion

    public void OpenFile() => s.OpenFileByTextReader(s.HostsFilePath);

    public void OpenFileDir() => s.OpenFolder(s.HostsFilePath);

    public async Task<bool> ResetFile()
    {
        if (Startup.Instance.IsMainProcess)
        {
            var r = await MessageBox.ShowAsync(
                AppResources.CommunityFix_ResetHostsFileTip,
                AppResources.Warning,
                button: MessageBox.Button.OKCancel,
                rememberChooseKey: MessageBox.DontPromptType.ResetHostsFile);
            if (r.IsOK())
            {
                try
                {
                    var isOk = true;
                    var privilegedThis = await GetPrivilegedThisAsync();
                    if (privilegedThis != null)
                    {
                        isOk = await privilegedThis.ResetFile();
                    }
                    else
                    {
#if WINDOWS
                        watcher?.Dispose();
                        watcher = null;
#endif
                        s.WriteDefaultHostsContent();
                    }

                    if (isOk)
                        Toast.Show(ToastIcon.Success, AppResources.CommunityFix_ResetHostsFileOk);
                    else
                        Toast.Show(ToastIcon.Error, AppResources.CommunityFix_ResetHostsFileCatchTip_.Format("IPC fail"));
                }
                catch (Exception e)
                {
                    Log.Error(TAG, e, nameof(ResetFile));
                    Toast.Show(ToastIcon.Error, AppResources.CommunityFix_ResetHostsFileCatchTip_.Format(e.Message));
                }
            }
        }
        else
        {
            s.WriteDefaultHostsContent();
        }
        return true;
    }

    /// <summary>
    /// 获取 hosts 文件编码
    /// </summary>
    /// <returns></returns>
    Encoding GetEncoding() => GeneralSettings.HostsFileEncodingType.Value switch
    {
        EncodingType.ANSICodePage => s.Default,
        EncodingType.UTF8 => Encoding.Default,
        EncodingType.UTF8WithBOM => Encoding.UTF8,
        _ => OperatingSystem.IsWindows() ? Encoding.UTF8 : Encoding.Default,
    };

    static Dictionary<string, string> ReadHostsAllLines(StreamReader? fileReader)
    {
        Dictionary<string, string> dict = new();
        if (fileReader == null) return dict;
        int index = 0;
        static bool Contains(Dictionary<string, string> d, string v) => d.ContainsKey(v);
        while (true)
        {
            index++;
            var line = fileReader.ReadLine();
            if (line == null) break;
            if (string.IsNullOrWhiteSpace(line)) continue;
            var is_effective_value_v2 = HandleLineV2(Contains, index, dict, line, out var array);
            if (is_effective_value_v2 == HandleLineResult.Duplicate)
            {
                dict[array[1]] = array[0];
            }
            else
            {
                var is_effective_value = Convert(is_effective_value_v2);
                if (!is_effective_value.HasValue || !is_effective_value.Value) continue;
                dict.Add(array[1], array[0]);
            }
        }
        return dict;
    }

    public OperationResult<List<(string ip, string domain)>> ReadHostsAllLines()
    {
        lock (lockObj)
        {
            static IEnumerable<(string ip, string domain)> ReadHostsAllLines_(StreamReader? fileReader)
            {
                var value = ReadHostsAllLines(fileReader);
                return value.Select(x => (x.Value, x.Key));
            }
            OperationResult<List<(string ip, string domain)>> result;
            if (!TryOperation(out var errmsg, out var fileInfo, out var _))
            {
                result = new(OperationResultType.Error, errmsg);
                return result;
            }
            try
            {
                var result_read = TryReadHosts(() => fileInfo.OpenText(GetEncoding()), nameof(HandleHosts));
                if (result_read.ResultType != OperationResultType.Success)
                {
                    return result = new(result_read.ResultType, result_read.Message);
                }
                using var fileReader = result_read.AppendData;
                result = new()
                {
                    AppendData = new(ReadHostsAllLines_(fileReader)),
                    ResultType = OperationResultType.Success,
                    Message = AppResources.Hosts_ReadSuccess,
                };
            }
            catch (Exception ex)
            {
                Log.Error(TAG, ex, "ReadHostsAllLines catch.");
                result = new(OperationResultType.Error, ex.GetAllMessage());
            }
            return result;
        }
    }

    public IOperationResult<Dictionary<string, string>> ReadHostsAllLinesV2()
    {
        lock (lockObj)
        {
            OperationResult<Dictionary<string, string>> result;
            if (!TryOperation(out var errmsg, out var fileInfo, out var _))
            {
                result = new(OperationResultType.Error, errmsg);
                return result;
            }
            try
            {
                var result_read = TryReadHosts(() => fileInfo.OpenText(GetEncoding()), nameof(HandleHosts));
                if (result_read.ResultType != OperationResultType.Success)
                {
                    return result = new(result_read.ResultType, result_read.Message);
                }
                using var fileReader = result_read.AppendData;
                result = new()
                {
                    AppendData = ReadHostsAllLines(fileReader),
                    ResultType = OperationResultType.Success,
                    Message = AppResources.Hosts_ReadSuccess,
                };
            }
            catch (Exception ex)
            {
                Log.Error(TAG, ex, "ReadHostsAllLinesV2 catch.");
                result = new(OperationResultType.Error, ex.GetAllMessage());
            }
            return result;
        }
    }

    static OperationResult GetOperationResultByIpcResult(OperationResult? result)
    {
        if (result == null)
            return new OperationResult
            {
                ResultType = OperationResultType.Error,
                Message = "Ipc call error",
            };
        return result;
    }

    public async Task<OperationResult> UpdateHosts(string ip, string domain)
    {
        var privilegedThis = await GetPrivilegedThisAsync();
        if (privilegedThis != null)
        {
            var r = await privilegedThis.UpdateHosts(ip, domain);
            return GetOperationResultByIpcResult(r);
        }
        var dict = new Dictionary<string, string>
            {
                { domain, ip },
            };
        var r2 = await UpdateHosts(dict);
        return r2;
    }

    public async Task<OperationResult> UpdateHosts(IEnumerable<(string ip, string domain)> hosts)
    {
        var privilegedThis = await GetPrivilegedThisAsync();
        if (privilegedThis != null)
        {
            var r = await privilegedThis.UpdateHosts(hosts);
            return GetOperationResultByIpcResult(r);
        }
        //var value = hosts.ToDictionary(k => k.domain, v => v.ip);
        var value = new Dictionary<string, string>();
        foreach (var (ip, domain) in hosts)
            value.TryAdd(domain, ip);
        var r2 = await UpdateHosts(value);
        return r2;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    static OperationResult Convert(IOperationResult result)
    {
        if (result is OperationResult operationResult)
            return operationResult;
        return new OperationResult(result);
    }

    public async Task<OperationResult> UpdateHosts(IReadOnlyDictionary<string, string> hosts)
    {
        var privilegedThis = await GetPrivilegedThisAsync();
        if (privilegedThis != null)
        {
            var r = await privilegedThis.UpdateHosts(hosts);
            return GetOperationResultByIpcResult(r);
        }

#if WINDOWS
        updateHostsValue.Clear();
        foreach (var item in hosts)
        {
            updateHostsValue.TryAdd(item.Key, item.Value);
        }
#endif

        var result = HandleHosts(isUpdateOrRemove: true, hosts);
#if WINDOWS
        StartWatcher();
#endif
        return Convert(result);
    }

    public async Task<OperationResult> RemoveHosts(string ip, string domain)
    {
        var privilegedThis = await GetPrivilegedThisAsync();
        if (privilegedThis != null)
        {
            var r = await privilegedThis.RemoveHosts(ip, domain);
            return GetOperationResultByIpcResult(r);
        }
        var hosts = new Dictionary<string, string>
            {
                { domain, ip },
            };
        var result = HandleHosts(isUpdateOrRemove: false, hosts);
        return Convert(result);
    }

    public async Task<OperationResult> RemoveHosts(string domain) => await RemoveHosts(string.Empty, domain);

    public async Task<OperationResult> RemoveHostsByTag()
    {
        var privilegedThis = await GetPrivilegedThisAsync();
        if (privilegedThis != null)
        {
            var r = await privilegedThis.RemoveHostsByTag();
            return GetOperationResultByIpcResult(r);
        }
#if WINDOWS
        watcher?.Dispose();
        watcher = null;
#endif
        var result = HandleHosts(isUpdateOrRemove: false);
        return Convert(result);
    }

    static IOperationResult<T> TryReadHosts<T>(Func<T> func, string methodName)
    {
        try
        {
            var r = func();
            return new OperationResult<T>(OperationResultType.Success, string.Empty, r);
        }
        catch (IOException ex)
        {
            Log.Error(TAG, ex, "TryReadHosts fail, methodName: {0}.", methodName);
            return new OperationResult<T>(OperationResultType.Error, AppResources.CommunityFix_ReadHosts_IOException);
        }
    }

    public bool ContainsHostsByTag()
    {
        try
        {
            var filePath = s.HostsFilePath;
            var fileInfo = new FileInfo(filePath);
            if (fileInfo.Exists && fileInfo.Length <= MaxFileLength)
            {
                var result = TryReadHosts(() => File.ReadAllLines(filePath, GetEncoding()), nameof(ContainsHostsByTag));
                if (result.ResultType == OperationResultType.Success)
                {
                    var lines = result.AppendData;
                    if (lines.Reverse().Any(x => x.StartsWith(MarkEnd)))
                    {
                        return true;
                    }
                }
                else
                {
                    Toast.Show(ToastIcon.Error, AppResources.OperationHostsError_.Format(result.Message));
                }
            }
        }
        catch (Exception ex)
        {
            Log.Error(TAG, ex, "ContainsHostsByTag fail.");
            Toast.Show(ToastIcon.Error, AppResources.OperationHostsError_.Format(ex.GetAllMessage()));
        }
        return false;
    }

    bool mOnExitRestoreHosts;
    readonly AsyncLock mOnExitRestoreHostsLock = new();

    public async Task OnExitRestoreHosts()
    {
        using (await mOnExitRestoreHostsLock.LockAsync())
        {
            if (mOnExitRestoreHosts) return;
            await RemoveHostsByTag();
            mOnExitRestoreHosts = true;
        }
    }

#if DEBUG
    static FileStream? mOccupyHostsFileStream;

    public async Task<bool> OccupyHosts()
    {
        var isOK = true;
        var privilegedThis = await GetPrivilegedThisAsync();
        if (privilegedThis != null)
        {
            // Ipc 调用失败时将返回默认值
            isOK = await privilegedThis.OccupyHosts();
        }
        mOccupyHostsFileStream = new FileStream(s.HostsFilePath, FileMode.Open, FileAccess.ReadWrite, FileShare.None);
        return isOK;
    }
#endif

    sealed class HandleHostsFileException : Exception
    {
        public const int Code_CommunityFix_Hosts_MarkDuplicate_ = 5;

        public HandleHostsFileException(string message) : base(message)
        {

        }

        public int Code { get; init; }
    }

#endif
}