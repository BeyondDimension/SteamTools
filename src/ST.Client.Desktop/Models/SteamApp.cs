using ReactiveUI;
using System.Application.Services;
using System.Application.UI;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using static System.Application.SteamApiUrls;

namespace System.Application.Models
{
    public class SteamApp : ReactiveObject, IComparable<SteamApp>
    {
        public SteamApp() { }

        public SteamApp(uint appid)
        {
            AppId = appid;
        }

        private const string NodeAppInfo = "appinfo";

        private const string NodeAppType = "type";

        private const string NodeCommon = "common";

        private const string NodeId = "gameid";

        private const string NodeName = "name";

        private const string NodeParentId = "parent";

        private const string NodePlatforms = "oslist";

        private const string NodePlatformsLinux = "linux";

        private const string NodePlatformsMac = "mac";

        private const string NodePlatformsWindows = "windows";

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

        public string? Name { get; set; }

        public string? EditName
        {
            get
            {
                if (_cachedName == null)
                {
                    _cachedName = _properties?.GetPropertyValue<string>(null, new string[]
                    {
                        NodeAppInfo,
                        NodeCommon,
                        "name"
                    });
                }
                return _cachedName;
            }
            set
            {
                _properties?.SetPropertyValue(SteamAppPropertyType.String, value, new string[]
                {
                    NodeAppInfo,
                    NodeCommon,
                    "name"
                });
                ClearCachedProps();
            }
        }

        public string? DisplayName => string.IsNullOrEmpty(EditName) ? Name : EditName;

        public string? Logo { get; set; }

        public string? Icon { get; set; }

        public SteamAppType Type { get; set; }

        public uint ParentId { get; set; }

        public long lastUpdatedTicks;
        /// <summary>
        /// 最后更新日期
        /// </summary>
        public DateTime LastUpdated { get; set; }

        /// <summary>
        /// 占用硬盘字节大小
        /// </summary>
        public long SizeOnDisk { get; set; }

        /// <summary>
        /// 最后运行用户SteamId64
        /// </summary>
        public long LastOwner { get; set; }

        /// <summary>
        /// 下载字节数
        /// </summary>
        public long BytesToDownload { get; set; }

        /// <summary>
        /// 已下载字节数 
        /// </summary>
        public long BytesDownloaded { get; set; }


        public IList<uint> ChildApp { get; set; } = new List<uint>();

        public string? LogoUrl => string.IsNullOrEmpty(Logo) ? null :
            string.Format(STEAMAPP_LOGO_URL, AppId, Logo);

        public string LibraryLogoUrl => string.Format(STEAMAPP_LIBRARY_URL, AppId);

        public Task<string> LibraryLogoStream => ISteamService.Instance.GetAppImageAsync(this, SteamApp.LibCacheType.Library_600x900);

        public string LibraryHeaderUrl => string.Format(STEAMAPP_LIBRARYHERO_URL, AppId);

        private string? _LibraryHeaderStream;
        public string? LibraryHeaderStream
        {
            get => _LibraryHeaderStream;
            set => this.RaiseAndSetIfChanged(ref _LibraryHeaderStream, value);
        }
        public string LibraryHeaderBlurUrl => string.Format(STEAMAPP_LIBRARYHEROBLUR_URL, AppId);

        private string? _LibraryHeaderBlurStream;
        public string? LibraryHeaderBlurStream
        {
            get => _LibraryHeaderBlurStream;
            set => this.RaiseAndSetIfChanged(ref _LibraryHeaderBlurStream, value);
        }

        public string LibraryNameUrl => string.Format(STEAMAPP_LIBRARYLOGO_URL, AppId);

        private string? _LibraryNameStream;
        public string? LibraryNameStream
        {
            get => _LibraryNameStream;
            set => this.RaiseAndSetIfChanged(ref _LibraryNameStream, value);
        }


        public string HeaderLogoUrl => string.Format(STEAMAPP_CAPSULE_URL, AppId);

        private string? _HeaderLogoStream;
        public string? HeaderLogoStream
        {
            get => _HeaderLogoStream;
            set => this.RaiseAndSetIfChanged(ref _HeaderLogoStream, value);
        }

        public string? IconUrl => string.IsNullOrEmpty(Icon) ? null :
            string.Format(STEAMAPP_LOGO_URL, AppId, Icon);

        private Process? _Process;
        public Process? Process
        {
            get => _Process;
            set => this.RaiseAndSetIfChanged(ref _Process, value);
        }

        //public TradeCard? Card { get; set; }

        //public SteamAppInfo? Common { get; set; }

        public string GetIdAndName()
        {
            return $"{AppId} | {Name}";
        }

        public int CompareTo(SteamApp? other) => string.Compare(Name, other?.Name);

        private string? _cachedName;

        private bool? _cachedHasSortAs;

        private string? _cachedSortAs;

        private byte[] _stuffBeforeHash;

        private uint _changeNumber;

        private byte[] _originalData;

        private SteamAppPropertyTable _properties;

        private void ClearCachedProps()
        {
            _cachedName = null;
            _cachedSortAs = null;
            _cachedHasSortAs = null;
        }

        public event EventHandler Modified;

        private void OnEntryModified(object sender, EventArgs e)
        {
            EventHandler modified = Modified;
            if (modified == null)
            {
                return;
            }
            modified(this, new EventArgs());
        }

        public Process? StartSteamAppProcess()
        {
            return Process = IDesktopPlatformService.Instance.OpenProcess(AppHelper.ProgramPath, "-clt app -silence -id " + AppId.ToString());
        }

        public static SteamApp? FromReader(BinaryReader reader)
        {
            uint num = reader.ReadUInt32();
            if (num == 0)
            {
                return null;
            }
            SteamApp app = new()
            {
                AppId = num,
            };
            try
            {
                int count = reader.ReadInt32();
                byte[] array = reader.ReadBytes(count);
                using BinaryReader binaryReader = new(new MemoryStream(array));
                app._stuffBeforeHash = binaryReader.ReadBytes(16);
                binaryReader.ReadBytes(20);
                app._changeNumber = binaryReader.ReadUInt32();
                app._properties = binaryReader.ReadPropertyTable();
                var nodes = new string[3] { NodeAppInfo, NodeCommon, string.Empty };
                //var installpath = app._properties.GetPropertyValue<string>(null, new string[]
                //{
                //    NodeAppInfo,
                //    "config",
                //    "installdir"
                // });
                //if (!string.IsNullOrEmpty(installpath))
                //{
                //    app.InstalledDir = Path.Combine(Services.ISteamService.Instance.SteamDirPath, "steamapps", NodeCommon, installpath);
                //}

                nodes[2] = NodeParentId;
                app.ParentId = (uint)app._properties.GetPropertyValue<int>(0, nodes);

                nodes[2] = NodeAppType;
                string type = app._properties.GetPropertyValue<string>("", nodes);

                if (Enum.TryParse(type, true, out SteamAppType apptype))
                {
                    app.Type = apptype;
                }
                else
                {
                    app.Type = SteamAppType.Unknown;
                    Debug.WriteLine(string.Format("AppInfo: New AppType '{0}'", type));
                }


                nodes[2] = NodePlatforms;
                string oslist = app._properties.GetPropertyValue<string>("", NodePlatforms);



                var propertyValue = app._properties.GetPropertyValue<string>("", new string[]
                {
                        "appinfo",
                        "steam_edit",
                        "base_name"
                });
                if (propertyValue != "")
                {
                    nodes[2] = NodeName;
                    app._properties.SetPropertyValue(SteamAppPropertyType.String, propertyValue, nodes);
                }
                var propertyValue2 = app._properties.GetPropertyValue<string>("", new string[]
                {
                        "appinfo",
                        "steam_edit",
                        "base_type"
                });
                if (propertyValue2 != "")
                {
                    nodes[2] = NodeAppType;
                    app._properties.SetPropertyValue(SteamAppPropertyType.String, propertyValue2, nodes);
                }
                app._originalData = array;
                app.ClearCachedProps();

            }
            catch (Exception ex)
            {
                Log.Error(nameof(SteamApp), ex, string.Format("Failed to load entry with appId {0}", app.AppId));
            }
            return app;
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
            /// <see cref="LibraryLogoUrl"/>
            /// </summary>
            Library_600x900,

            /// <summary>
            /// <see cref="LibraryHeaderUrl"/>
            /// </summary>
            Library_Hero,

            /// <summary>
            /// <see cref="LibraryHeaderBlurUrl"/>
            /// </summary>
            Library_Hero_Blur,

            /// <summary>
            /// <see cref="LibraryNameUrl"/>
            /// </summary>
            Logo,
        }
    }
}