using ReactiveUI;
using System.Application.Services;
using System.Application.UI;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using static System.Application.SteamApiUrls;

namespace System.Application.Models
{
    public class SteamApp : ReactiveObject, IComparable<SteamApp>
    {
        private const string NodeAppInfo = "appinfo";

        private const string NodeAppType = "type";

        private const string NodeCommon = "common";

        private const string NodeConfig = "config";

        private const string NodeExtended = "extended";

        private const string NodeId = "gameid";

        private const string NodeName = "name";

        private const string NodeParentId = "parent";

        private const string NodePlatforms = "oslist";

        private const string NodePlatformsLinux = "linux";

        private const string NodePlatformsMac = "mac";

        private const string NodePlatformsWindows = "windows";

        private const string NodeSortAs = "sortas";

        private const string NodeDeveloper = "developer";

        private const string NodePublisher = "publisher";

        private const string NodeLaunch = "launch";

        public SteamApp() { }

        public SteamApp(uint appid)
        {
            AppId = appid;
        }

        public int Index { get; set; }

        public int State { get; set; }

        /// <summary>
        /// Returns a value indicating whether the game is being downloaded.
        /// </summary>
        public bool IsDownloading => CheckDownloading(State);

        public uint AppId { get; set; }

        //public bool IsInstalled { get; set; }
        public bool IsInstalled => IsBitSet(State, 2);

        public string? InstalledDrive => !string.IsNullOrEmpty(InstalledDir) ? Path.GetPathRoot(InstalledDir)?.ToUpper()?.Replace(Path.DirectorySeparatorChar.ToString(), "") : null;

        public string? InstalledDir { get; set; }

        public string? BaseName { get; set; }

        string? _Name;

        public string? Name
        {
            get => _Name;
            set
            {
                if (_Name != value)
                {
                    _Name = value;
                    SortAs = value;
                    this.RaisePropertyChanged();
                }
            }
        }

        string? _SortAs;

        public string? SortAs
        {
            get => _SortAs;
            set
            {
                if (_SortAs != value)
                {
                    _SortAs = value;
                    this.RaisePropertyChanged();
                }
            }
        }

        public string? Developer { get; set; }

        public string? Publisher { get; set; }

        public uint? SteamReleaseDate { get; set; }

        public uint? OriginReleaseDate { get; set; }

        public string? OSList { get; set; }

        #region 暂时不用
        //public string? EditName
        //{
        //    get
        //    {
        //        return _properties?.GetPropertyValue<string>(null, NodeAppInfo,
        //            NodeCommon,
        //            NodeName);
        //    }
        //    set
        //    {
        //        _properties?.SetPropertyValue(SteamAppPropertyType.String, value, NodeAppInfo,
        //            NodeCommon,
        //            NodeName);
        //    }
        //}

        //public string? EditSortAs
        //{
        //    get
        //    {
        //        return this._properties?.GetPropertyValue<string>(this.Name, NodeAppInfo, NodeCommon, NodeSortAs);
        //    }
        //    set
        //    {
        //        _properties?.SetPropertyValue(SteamAppPropertyType.String, value, NodeAppInfo, NodeCommon, NodeSortAs);
        //    }
        //}

        //public string? EditDeveloper
        //{
        //    get
        //    {
        //        return _properties?.GetPropertyValue<string>(null, NodeAppInfo,
        //             NodeExtended,
        //             NodeDeveloper);
        //    }
        //    set
        //    {
        //        _properties?.SetPropertyValue(SteamAppPropertyType.String, value, NodeAppInfo,
        //            NodeExtended,
        //            NodeDeveloper);
        //    }
        //}

        //public string? EditPublisher
        //{
        //    get
        //    {
        //        return _properties?.GetPropertyValue<string>(null, NodeAppInfo,
        //             NodeExtended,
        //             NodePublisher);
        //    }
        //    set
        //    {
        //        _properties?.SetPropertyValue(SteamAppPropertyType.String, value, NodeAppInfo,
        //            NodeExtended,
        //            NodePublisher);
        //    }
        //}
        #endregion

        public bool SetEditProperty(SteamApp appInfo)
        {
            if (_properties != null)
            {
                #region steam_edit Data
                //SteamAppPropertyTable? propertyValue = _properties.GetPropertyValue<SteamAppPropertyTable>(null, NodeAppInfo, NodeCommon);
                //if (propertyValue != null)
                //{
                //    string? text = Name;
                //    if (text != null)
                //    {
                //        SteamAppPropertyTable? propertyValue2 = propertyValue.GetPropertyValue<SteamAppPropertyTable>(null, "name_localized");
                //        if (propertyValue2 != null)
                //        {
                //            _properties.SetPropertyValue(SteamAppPropertyType.Table, propertyValue2, NodeAppInfo, "steam_edit", "base_name_localized");
                //            propertyValue.RemoveProperty("name_localized");
                //        }
                //        _properties.SetPropertyValue(SteamAppPropertyType.String, text, NodeAppInfo, "steam_edit", "base_name");
                //        //if (SteamData.UseCompleted && this.IsCompleted)
                //        //{
                //        //    text += SteamData.CompletedSuffix;
                //        //}
                //        propertyValue.SetPropertyValue("name", SteamAppPropertyType.String, text);
                //    }
                //    else
                //    {
                //        Log.Info("SteamApp Write Error", $"AppInfo {AppId:X8} has null name!");
                //    }

                //    //string? text2 = propertyValue.GetPropertyValue<string>("type", "game");
                //    //if (text2 != null)
                //    //{
                //    //    _properties.SetPropertyValue(SteamAppPropertyType.String, text2, NodeAppInfo, "steam_edit", "base_type");
                //    //    //if (SteamData.UseHidden && this.IsHidden)
                //    //    //{
                //    //    //    text2 = "hidden_" + text2;
                //    //    //}
                //    //    propertyValue.SetPropertyValue("type", SteamAppPropertyType.String, text2);
                //    //}
                //    //else
                //    //{
                //    //    Log.Info("SteamApp Write Error", $"AppInfo {AppId:X8} has null type!");
                //    //}
                //}
                #endregion

                //SortAs = appInfo.SortAs;
                //Developer = appInfo.Developer;
                //Publisher = appInfo.Publisher;
                //LaunchItems = appInfo.LaunchItems;

                if (!string.IsNullOrEmpty(appInfo.Name) && appInfo.Name != appInfo.BaseName)
                {
                    Name = appInfo.Name;

                    SteamAppPropertyTable propertyValue2 = _properties.GetPropertyValue<SteamAppPropertyTable>(null, NodeAppInfo, NodeCommon, "name_localized");
                    if (propertyValue2 != null)
                    {
                        _properties.SetPropertyValue(SteamAppPropertyType.Table, propertyValue2, NodeAppInfo, "steam_edit", "base_name_localized");

                        //_properties.RemoveProperty(NodeAppInfo, NodeCommon, "name_localized"); 
                        _properties.SetPropertyValue(SteamAppPropertyType.Table, new SteamAppPropertyTable(), NodeAppInfo, NodeCommon, "name_localized");
                    }
                    _properties.SetPropertyValue(SteamAppPropertyType.String, appInfo.BaseName, NodeAppInfo, "steam_edit", "base_name");

                    _properties.SetPropertyValue(SteamAppPropertyType.String, appInfo.Name, NodeAppInfo, NodeCommon, NodeName);
                }
                else
                {
                    Log.Info("SteamApp Write Error", $"AppInfo {AppId:X8} has null name!");
                }

                //_properties.SetPropertyValue(SteamAppPropertyType.String, appInfo.Name, NodeAppInfo,
                //    NodeCommon,
                //    NodeName);

                _properties.SetPropertyValue(SteamAppPropertyType.String, appInfo.SortAs, NodeAppInfo,
                    NodeCommon,
                    NodeSortAs);

                _properties.SetPropertyValue(SteamAppPropertyType.String, appInfo.Developer, NodeAppInfo,
                    NodeExtended,
                    NodeDeveloper);

                _properties.SetPropertyValue(SteamAppPropertyType.String, appInfo.Developer, NodeAppInfo,
                    NodeCommon,
                    "associations", "0", "name");

                _properties.SetPropertyValue(SteamAppPropertyType.String, appInfo.Publisher, NodeAppInfo,
                    NodeExtended,
                    NodePublisher);

                _properties.SetPropertyValue(SteamAppPropertyType.String, appInfo.Publisher, NodeAppInfo,
                    NodeCommon,
                    "associations", "1", "name");

                if (appInfo.LaunchItems.Any_Nullable())
                {
                    var launchTable = new SteamAppPropertyTable();

                    foreach (var item in appInfo.LaunchItems)
                    {
                        var propertyTable = new SteamAppPropertyTable();

                        propertyTable.SetPropertyValue("executable", SteamAppPropertyType.String, item.Executable);

                        if (!string.IsNullOrEmpty(item.Label))
                        {
                            propertyTable.SetPropertyValue("description", SteamAppPropertyType.String, item.Label);
                        }
                        if (!string.IsNullOrEmpty(item.Arguments))
                        {
                            propertyTable.SetPropertyValue("arguments", SteamAppPropertyType.String, item.Arguments);
                        }
                        if (!string.IsNullOrEmpty(item.WorkingDir))
                        {
                            propertyTable.SetPropertyValue("workingdir", SteamAppPropertyType.String, item.WorkingDir);
                        }
                        if (!string.IsNullOrEmpty(item.Platform))
                        {
                            propertyTable.SetPropertyValue(SteamAppPropertyType.String, item.Platform, NodeConfig, NodePlatforms);
                        }
                        launchTable.SetPropertyValue(launchTable.Count.ToString(), SteamAppPropertyType.Table, propertyTable);
                    }

                    _properties.SetPropertyValue(SteamAppPropertyType.Table, launchTable, NodeAppInfo, NodeConfig, NodeLaunch);
                }

                return true;
            }
            return false;
        }

        private bool _IsEdited;

        public bool IsEdited
        {
            get => _IsEdited;
            set => this.RaiseAndSetIfChanged(ref _IsEdited, value);
        }

        public string DisplayName => string.IsNullOrEmpty(Name) ? AppId.ToString() : Name;

        string? _baseDLSSVersion;

        public string? BaseDLSSVersion
        {
            get => _baseDLSSVersion;
            set
            {
                if (_baseDLSSVersion != value)
                {
                    _baseDLSSVersion = value;
                    this.RaisePropertyChanged();
                }
            }
        }

        string? _currentDLSSVersion;

        public string? CurrentDLSSVersion
        {
            get => _currentDLSSVersion;
            set
            {
                if (_currentDLSSVersion != value)
                {
                    _currentDLSSVersion = value;
                    this.RaisePropertyChanged();
                }
            }
        }

        public bool HasDLSS { get; set; }

        public string? Logo { get; set; }

        public string? Icon { get; set; }

        public SteamAppType Type { get; set; }

        public uint ParentId { get; set; }

        /// <summary>
        /// 是否支持Steam云存档
        /// </summary>
        public bool IsCloudArchive => CloudQuota > 0;

        /// <summary>
        /// 云存档字节大小
        /// </summary>
        public long CloudQuota { get; set; }

        /// <summary>
        /// 云存档文件数量上限
        /// </summary>
        public int CloudMaxnumFiles { get; set; }

        /// <summary>
        /// 最后运行用户SteamId64
        /// </summary>
        public long LastOwner { get; set; }

        /// <summary>
        /// 最后更新日期
        /// </summary>
        public DateTime LastUpdated { get; set; }

        private long _SizeOnDi;

        /// <summary>
        /// 占用硬盘字节大小
        /// </summary>
        public long SizeOnDisk
        {
            get => _SizeOnDi;
            set => this.RaiseAndSetIfChanged(ref _SizeOnDi, value);
        }

        /// <summary>
        /// 需要下载字节数
        /// </summary>
        public long BytesToDownload { get; set; }

        /// <summary>
        /// 已下载字节数 
        /// </summary>
        public long BytesDownloaded { get; set; }

        //public int DownloadedProgressValue => IOPath.GetProgressPercentage(BytesDownloaded, BytesToDownload);

        /// <summary>
        /// 需要安装字节数
        /// </summary>
        public long BytesToStage { get; set; }

        /// <summary>
        /// 已安装字节数 
        /// </summary>
        public long BytesStaged { get; set; }

        public IList<uint> ChildApp { get; set; } = new List<uint>();

        private ObservableCollection<SteamAppLaunchItem>? _LaunchItems;

        public ObservableCollection<SteamAppLaunchItem>? LaunchItems
        {
            get => _LaunchItems;
            set => this.RaiseAndSetIfChanged(ref _LaunchItems, value);
        }

        private ObservableCollection<SteamAppSaveFile>? _SaveFiles;

        public ObservableCollection<SteamAppSaveFile>? SaveFiles
        {
            get => _SaveFiles;
            set => this.RaiseAndSetIfChanged(ref _SaveFiles, value);
        }

        public string? LogoUrl => string.IsNullOrEmpty(Logo) ? null :
            string.Format(STEAMAPP_LOGO_URL, AppId, Logo);

        public string LibraryGridUrl => string.Format(STEAMAPP_LIBRARY_URL, AppId);

        public Task<string> LibraryGridStream => ISteamService.Instance.GetAppImageAsync(this, LibCacheType.Library_Grid);

        private Stream? _EditLibraryGridStream;

        public Stream? EditLibraryGridStream
        {
            get => _EditLibraryGridStream;
            set => this.RaiseAndSetIfChanged(ref _EditLibraryGridStream, value);
        }

        public string LibraryHeroUrl => string.Format(STEAMAPP_LIBRARYHERO_URL, AppId);

        public Task<string> LibraryHeroStream => ISteamService.Instance.GetAppImageAsync(this, LibCacheType.Library_Hero);

        private Stream? _EditLibraryHeroStream;

        public Stream? EditLibraryHeroStream
        {
            get => _EditLibraryHeroStream;
            set => this.RaiseAndSetIfChanged(ref _EditLibraryHeroStream, value);
        }

        public string LibraryHeroBlurUrl => string.Format(STEAMAPP_LIBRARYHEROBLUR_URL, AppId);

        public Task<string> LibraryHeroBlurStream => ISteamService.Instance.GetAppImageAsync(this, LibCacheType.Library_Hero_Blur);

        public string LibraryLogoUrl => string.Format(STEAMAPP_LIBRARYLOGO_URL, AppId);

        public Task<string> LibraryLogoStream => ISteamService.Instance.GetAppImageAsync(this, LibCacheType.Logo);

        private Stream? _EditLibraryLogoStream;

        public Stream? EditLibraryLogoStream
        {
            get => _EditLibraryLogoStream;
            set => this.RaiseAndSetIfChanged(ref _EditLibraryLogoStream, value);
        }

        public string HeaderLogoUrl => string.Format(STEAMAPP_HEADIMAGE_URL, AppId);

        public Task<string> HeaderLogoStream => ISteamService.Instance.GetAppImageAsync(this, LibCacheType.Header);

        private Stream? _EditHeaderLogoStream;

        public Stream? EditHeaderLogoStream
        {
            get => _EditHeaderLogoStream;
            set => this.RaiseAndSetIfChanged(ref _EditHeaderLogoStream, value);
        }

        public string CAPSULELogoUrl => string.Format(STEAMAPP_CAPSULE_URL, AppId);

        public string? IconUrl => string.IsNullOrEmpty(Icon) ? null :
            string.Format(STEAMAPP_LOGO_URL, AppId, Icon);

        private Process? _Process;

        public Process? Process
        {
            get => _Process;
            set => this.RaiseAndSetIfChanged(ref _Process, value);
        }

        private bool _IsWatchDownloading;

        public bool IsWatchDownloading
        {
            get => _IsWatchDownloading;
            set => this.RaiseAndSetIfChanged(ref _IsWatchDownloading, value);
        }

        //public TradeCard? Card { get; set; }

        //public SteamAppInfo? Common { get; set; }

        public string GetIdAndName()
        {
            return $"{AppId} | {DisplayName}";
        }

        public int CompareTo(SteamApp? other) => string.Compare(Name, other?.Name);

        private byte[]? _stuffBeforeHash;

        private uint _changeNumber;

        private byte[] _originalData;

        public byte[] OriginalData { get => _originalData; set => _originalData = value; }

        private SteamAppPropertyTable? _properties;

        public SteamAppPropertyTable? ChangesData => _properties;

        //public event EventHandler? Modified;

        //private void OnEntryModified(object sender, EventArgs e)
        //{
        //    var modified = Modified;
        //    if (modified == null)
        //    {
        //        return;
        //    }
        //    modified(this, new EventArgs());
        //}

        public Process? StartSteamAppProcess()
        {
            if (OperatingSystem2.IsWindows)
            {
                return Process = Process2.Start(
                    IApplication.ProgramPath,
                    $"-clt app -silence -id {AppId}");
            }
            else if (OperatingSystem2.IsLinux)
            {
                return Process = Process2.Start($" SteamAppId={AppId} | {IApplication.ProgramPath} -clt app -silence -id {AppId}");
            }
            else
            {
                return Process = Process2.StartThis(AppId, $"{IApplication.ProgramPath} -clt app -silence -id {AppId}");
            }
        }

        #region Replace DLSS dll files methods

        //        public void DetectDLSS()
        //        {
        //            BaseDLSSVersion = string.Empty;
        //            CurrentDLSSVersion = "N/A";
        //            var dlssDlls = Directory.GetFiles(InstalledDir!, "nvngx_dlss.dll", SearchOption.AllDirectories);
        //            if (dlssDlls.Length > 0)
        //            {
        //                HasDLSS = true;

        //                // TODO: Handle a single folder with various versions of DLSS detected.
        //                // Currently we are just using the first.

        //                foreach (var dlssDll in dlssDlls)
        //                {
        //                    var dllVersionInfo = FileVersionInfo.GetVersionInfo(dlssDll);
        //                    CurrentDLSSVersion = dllVersionInfo.FileVersion?.Replace(",", ".") ?? string.Empty;
        //                    break;
        //                }

        //                dlssDlls = Directory.GetFiles(InstalledDir!, "nvngx_dlss.dll.dlsss", SearchOption.AllDirectories);
        //                if (dlssDlls.Length > 0)
        //                {
        //                    foreach (var dlssDll in dlssDlls)
        //                    {
        //                        var dllVersionInfo = FileVersionInfo.GetVersionInfo(dlssDll);
        //                        BaseDLSSVersion = dllVersionInfo.FileVersion?.Replace(",", ".") ?? string.Empty;
        //                        break;
        //                    }
        //                }
        //            }
        //            else
        //            {
        //                HasDLSS = false;
        //            }
        //        }

        //        internal bool ResetDll()
        //        {
        //            var foundDllBackups = Directory.GetFiles(InstalledDir!, "nvngx_dlss.dll.dlsss", SearchOption.AllDirectories);
        //            if (foundDllBackups.Length == 0)
        //            {
        //                return false;
        //            }

        //            var versionInfo = FileVersionInfo.GetVersionInfo(foundDllBackups.First());
        //            var resetToVersion = $"{versionInfo.FileMajorPart}.{versionInfo.FileMinorPart}.{versionInfo.FileBuildPart}.{versionInfo.FilePrivatePart}";

        //            foreach (var dll in foundDllBackups)
        //            {
        //                try
        //                {
        //                    var dllPath = Path.GetDirectoryName(dll);
        //                    var targetDllPath = Path.Combine(dllPath!, "nvngx_dlss.dll");
        //#if NETSTANDARD
        //                    File.Move(dll, targetDllPath);
        //#else
        //                    File.Move(dll, targetDllPath, true);
        //#endif
        //                }
        //                catch (Exception err)
        //                {
        //                    Debug.WriteLine($"ResetDll Error: {err.Message}");
        //                    return false;
        //                }
        //            }

        //            CurrentDLSSVersion = resetToVersion;
        //            BaseDLSSVersion = string.Empty;

        //            return true;
        //        }

        //        internal bool UpdateDll(LocalDlssDll localDll)
        //        {
        //            if (localDll == null)
        //            {
        //                return false;
        //            }

        //            var foundDlls = Directory.GetFiles(InstalledDir!, "nvngx_dlss.dll", SearchOption.AllDirectories);
        //            if (foundDlls.Length == 0)
        //            {
        //                return false;
        //            }

        //            var versionInfo = FileVersionInfo.GetVersionInfo(localDll.Filename);
        //            var targetDllVersion = $"{versionInfo.FileMajorPart}.{versionInfo.FileMinorPart}.{versionInfo.FileBuildPart}.{versionInfo.FilePrivatePart}";

        //            var baseDllVersion = string.Empty;

        //            // Backup old dlls.
        //            foreach (var dll in foundDlls)
        //            {
        //                var dllPath = Path.GetDirectoryName(dll);
        //                var targetDllPath = Path.Combine(dllPath!, "nvngx_dlss.dll.dlsss");
        //                if (File.Exists(targetDllPath) == false)
        //                {
        //                    try
        //                    {
        //                        var defaultVersionInfo = FileVersionInfo.GetVersionInfo(dll);
        //                        baseDllVersion = $"{defaultVersionInfo.FileMajorPart}.{defaultVersionInfo.FileMinorPart}.{defaultVersionInfo.FileBuildPart}.{defaultVersionInfo.FilePrivatePart}";

        //                        File.Copy(dll, targetDllPath, true);
        //                    }
        //                    catch (Exception err)
        //                    {
        //                        Debug.WriteLine($"UpdateDll Error: {err.Message}");
        //                        return false;
        //                    }
        //                }
        //            }

        //            foreach (var dll in foundDlls)
        //            {
        //                try
        //                {
        //                    File.Copy(localDll.Filename, dll, true);
        //                }
        //                catch (Exception err)
        //                {
        //                    Debug.WriteLine($"UpdateDll Error: {err.Message}");
        //                    return false;
        //                }
        //            }

        //            CurrentDLSSVersion = targetDllVersion;
        //            if (!string.IsNullOrEmpty(baseDllVersion))
        //            {
        //                BaseDLSSVersion = baseDllVersion;
        //            }
        //            return true;
        //        }

        #endregion

        public SteamApp ExtractReaderProperty(SteamAppPropertyTable properties, uint[]? installedAppIds = null)
        {
            if (properties != null)
            {
                //var installpath = properties.GetPropertyValue<string>(null, NodeAppInfo, NodeConfig, "installdir");

                //if (!string.IsNullOrEmpty(installpath))
                //{
                //    app.InstalledDir = Path.Combine(ISteamService.Instance.SteamDirPath, ISteamService.dirname_steamapps, NodeCommon, installpath);
                //}

                Name = properties.GetPropertyValue<string>(string.Empty, NodeAppInfo, NodeCommon, NodeName);
                SortAs = properties.GetPropertyValue<string>(string.Empty, NodeAppInfo, NodeCommon, NodeSortAs);
                if (!SortAs.Any_Nullable())
                {
                    SortAs = Name;
                }
                ParentId = properties.GetPropertyValue<uint>(0, NodeAppInfo, NodeCommon, NodeParentId);
                Developer = properties.GetPropertyValue<string>(string.Empty, NodeAppInfo, NodeExtended, NodeDeveloper);
                Publisher = properties.GetPropertyValue<string>(string.Empty, NodeAppInfo, NodeExtended, NodePublisher);
                //SteamReleaseDate = properties.GetPropertyValue<uint>(0, NodeAppInfo, NodeCommon, "steam_release_date");
                //OriginReleaseDate = properties.GetPropertyValue<uint>(0, NodeAppInfo, NodeCommon, "original_release_date");

                var type = properties.GetPropertyValue<string>(string.Empty, NodeAppInfo, NodeCommon, NodeAppType);
                if (Enum.TryParse(type, true, out SteamAppType apptype))
                {
                    Type = apptype;
                }
                else
                {
                    Type = SteamAppType.Unknown;
                    Debug.WriteLineIf(!string.IsNullOrEmpty(type), string.Format("AppInfo: New AppType '{0}'", type));
                }

                OSList = properties.GetPropertyValue(string.Empty, NodeAppInfo, NodeCommon, NodePlatforms);

                if (installedAppIds != null)
                {
                    if (installedAppIds.Contains(AppId) &&
                        (Type == SteamAppType.Application ||
                        Type == SteamAppType.Game ||
                        Type == SteamAppType.Tool ||
                        Type == SteamAppType.Demo))
                    {
                        // This is an installed app.
                        State = 4;
                    }
                }

                if (IsInstalled)
                {
                    var launchTable = properties.GetPropertyValue<SteamAppPropertyTable?>(null, NodeAppInfo, NodeConfig, NodeLaunch);

                    if (launchTable != null)
                    {
                        var launchItems = from table in (from prop in (from prop in launchTable.Properties
                                                                       where prop.PropertyType == SteamAppPropertyType.Table
                                                                       select prop).OrderBy((SteamAppProperty prop) => prop.Name, StringComparer.OrdinalIgnoreCase)
                                                         select prop.GetValue<SteamAppPropertyTable>())
                                          select new SteamAppLaunchItem
                                          {
                                              Label = table.GetPropertyValue<string?>("description"),
                                              Executable = table.GetPropertyValue<string?>("executable"),
                                              Arguments = table.GetPropertyValue<string?>("arguments"),
                                              WorkingDir = table.GetPropertyValue<string?>("workingdir"),
                                              Platform = table.TryGetPropertyValue<SteamAppPropertyTable>(NodeConfig, out var propertyTable) ?
                                              propertyTable.TryGetPropertyValue<string>(NodePlatforms, out var os) ? os : null : null,
                                          };

                        LaunchItems = new ObservableCollection<SteamAppLaunchItem>(launchItems.ToList());
                    }
                }

                CloudQuota = properties.GetPropertyValue<int>(0, NodeAppInfo, "ufs", "quota");
                CloudMaxnumFiles = properties.GetPropertyValue<int>(0, NodeAppInfo, "ufs", "maxnumfiles");

                var savefilesTable = properties.GetPropertyValue<SteamAppPropertyTable?>(null, NodeAppInfo, "ufs", "savefiles");

                if (savefilesTable != null)
                {
                    var savefiles = from table in (from prop in (from prop in savefilesTable.Properties
                                                                 where prop.PropertyType == SteamAppPropertyType.Table
                                                                 select prop).OrderBy((SteamAppProperty prop) => prop.Name, StringComparer.OrdinalIgnoreCase)
                                                   select prop.GetValue<SteamAppPropertyTable>())
                                    select new SteamAppSaveFile
                                    (
                                        AppId,
                                        table.GetPropertyValue<string?>("root"),
                                        table.GetPropertyValue<string?>("path"),
                                        table.GetPropertyValue<string?>("pattern")
                                    )
                                    {
                                        Recursive = table.GetPropertyValue<bool>(false, "recursive"),
                                    };

                    SaveFiles = new ObservableCollection<SteamAppSaveFile>(savefiles.ToList());
                }

                BaseName = properties.GetPropertyValue<string>(string.Empty, NodeAppInfo, "steam_edit", "base_name");

                if (string.IsNullOrEmpty(BaseName))
                {
                    BaseName = Name;
                }
            }
            return this;
        }

        public static SteamApp? FromReader(BinaryReader reader, uint[]? installedAppIds = null, bool isSaveProperties = false)
        {
            uint id = reader.ReadUInt32();
            if (id == 0)
            {
                return null;
            }
            SteamApp app = new()
            {
                AppId = id,
            };
            try
            {
                int count = reader.ReadInt32();
                byte[] array = reader.ReadBytes(count);
                using BinaryReader binaryReader = new(new MemoryStream(array));
                app._stuffBeforeHash = binaryReader.ReadBytes(16);
                binaryReader.ReadBytes(20);
                app._changeNumber = binaryReader.ReadUInt32();

                var properties = SteamAppPropertyHelper.ReadPropertyTable(binaryReader);

                if (properties == null)
                    return app;

                if (isSaveProperties)
                {
                    app._properties = properties;
                    app._originalData = array;
                }
                app.ExtractReaderProperty(properties, installedAppIds);
            }
            catch (Exception ex)
            {
                Log.Error(nameof(SteamApp), ex, string.Format("Failed to load entry with appId {0}", app.AppId));
            }
            return app;
        }

        public void Write(BinaryWriter writer)
        {
            if (_properties == null)
                throw new ArgumentNullException($"SteamApp Write Failed. {nameof(_properties)} is null.");
            SteamAppPropertyTable propertyTable = new SteamAppPropertyTable(this._properties);
            string s = propertyTable.ToString();
            byte[] bytes = Encoding.UTF8.GetBytes(s);
            byte[] buffer = SHA1.Create().ComputeHash(bytes);
            writer.Write((int)AppId);
            using BinaryWriter binaryWriter = new BinaryWriter(new MemoryStream());
            binaryWriter.Write(_stuffBeforeHash);
            binaryWriter.Write(buffer);
            binaryWriter.Write(_changeNumber);
            binaryWriter.Write(propertyTable);
            MemoryStream memoryStream = (MemoryStream)binaryWriter.BaseStream;
            writer.Write((int)memoryStream.Length);
            writer.Write(memoryStream.GetBuffer(), 0, (int)memoryStream.Length);
        }

        private static bool IsBitSet(int b, int pos)
        {
            return (b & (1 << pos)) != 0;
        }

        /// <summary>
        /// Returns a value indicating whether the game is being downloaded.
        /// </summary>
        public static bool CheckDownloading(int appState)
        {
            return (IsBitSet(appState, 1) || IsBitSet(appState, 10)) && !IsBitSet(appState, 9);

            /* Counting from zero and starting from the right
             * Bit 1 indicates if a download is running
             * Bit 3 indicates if a preloaded game download 
             * Bit 2 indicates if a game is installed
             * Bit 9 indicates if the download has been stopped by the user. The download will not happen, so don't wait for it.
             * Bit 10 (or maybe Bit 5) indicates if a DLC is downloaded for a game
             * 
             * All known stateFlags while a download is running so far:
             * 00000000110
             * 10000000010
             * 10000010010
             * 10000100110
             * 10000000110
             * 10000010100 Bit 1 not set, but Bit 5 and Bit 10. Happens if downloading a DLC for an already downloaded game.
             *             Because for a very short time after starting the download for this DLC the stateFlags becomes 20 = 00000010100
             *             I think Bit 5 indicates if "something" is happening with a DLC and Bit 10 indicates if it is downloading.
             */
        }

        public async void RefreshEditImage()
        {
            this.EditLibraryGridStream = IOPath.OpenRead(await this.LibraryGridStream);
            this.EditLibraryHeroStream = IOPath.OpenRead(await this.LibraryHeroStream);
            this.EditLibraryLogoStream = IOPath.OpenRead(await this.LibraryLogoStream);
            this.EditHeaderLogoStream = IOPath.OpenRead(await this.HeaderLogoStream);
        }

        public enum LibCacheType : byte
        {
            /// <summary>
            /// <see cref="HeaderLogoUrl"/>
            /// </summary>
            Header,

            /// <summary>
            /// <see cref="IconUrl"/>
            /// </summary>
            Icon,

            /// <summary>
            /// <see cref="LibraryGridUrl"/>
            /// </summary>
            Library_Grid,

            /// <summary>
            /// <see cref="LibraryHeroUrl"/>
            /// </summary>
            Library_Hero,

            /// <summary>
            /// <see cref="LibraryHeroBlurUrl"/>
            /// </summary>
            Library_Hero_Blur,

            /// <summary>
            /// <see cref="LibraryLogoUrl"/>
            /// </summary>
            Logo,
        }
    }
}