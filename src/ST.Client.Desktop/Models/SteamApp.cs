using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using static System.Application.SteamApiUrls;

namespace System.Application.Models
{
    public class SteamApp : IComparable<SteamApp>
    {
        public int ID { get; set; }
        public int Index { get; set; }

        public uint AppId { get; set; }

        public bool IsInstalled { get; set; }

        public string? InstalledDir { get; set; }

        public string? Name { get; set; }

        public string? Logo { get; set; }

        public string? Icon { get; set; }

        public SteamAppType Type { get; set; }

        public string? LogoUrl => string.IsNullOrEmpty(Logo) ? null :
            string.Format(STEAMAPP_LOGO_URL, AppId, Logo);

        public string HeaderLogoUrl => string.Format(STEAMAPP_CAPSULE_URL, AppId);

        public string? IconUrl => string.IsNullOrEmpty(Icon) ? null :
            string.Format(STEAMAPP_LOGO_URL, AppId, Icon);

        public Process? Process { get; set; }

        public TradeCard? Card { get; set; }

        public SteamAppInfo? Common { get; set; }
        public string GetIdAndName()
        {
            return $"{AppId} | {Name}";
        }

        public int CompareTo(SteamApp? other) => string.Compare(Name, other?.Name);



        private string _cachedName;

        private bool? _cachedHasSortAs;

        private string _cachedSortAs;

        private byte[] _stuffBeforeHash;

        private uint _changeNumber;

        private byte[] _originalData;

        private SteamAppPropertyTable _properties;

        private void ClearCachedProps()
        {
            this._cachedName = null;
            this._cachedSortAs = null;
            this._cachedHasSortAs = null;
        }

        private IList<SteamApp> _entries = new List<SteamApp>();

        public event EventHandler Modified;
        private void OnEntryModified(object sender, EventArgs e)
        {
            EventHandler modified = this.Modified;
            if (modified == null)
            {
                return;
            }
            modified(this, new EventArgs());
        }
        public IEnumerable<SteamApp> Entries
        {
            get
            {
                return this._entries.Cast<SteamApp>();
            }
        }

        private uint _unknownValueAtStart;

        public static SteamApp? Load(string path)
        {
            SteamApp? app = null;
            if (!File.Exists(path))
            {
                return app;
            }
            using BinaryReader binaryReader = new(File.OpenRead(path));
            uint num = binaryReader.ReadUInt32();
            if (num != 123094055U)
            {
                Log.Error(nameof(SteamApp), string.Format("\"{0}\" magic code is not supported: 0x{1:X8}", Path.GetFileName(path), num));
                return app;
            }
            SteamApp appInfo = new();
            appInfo._unknownValueAtStart = binaryReader.ReadUInt32();
            while ((app = SteamApp.FromReader(binaryReader)) != null)
            {
                app._entries.Add(app);
                app.Modified += app.OnEntryModified;
            }
            return app;
        }

        public static SteamApp? FromReader(BinaryReader reader)
        {
            int num = reader.ReadInt32();
            if (num == 0)
            {
                return null;
            }
            SteamApp app = new SteamApp
            {
                ID = num
            };
            try
            {
                int count = reader.ReadInt32();
                byte[] array = reader.ReadBytes(count);
                using (BinaryReader binaryReader = new BinaryReader(new MemoryStream(array)))
                {
                    app._stuffBeforeHash = binaryReader.ReadBytes(16);
                    binaryReader.ReadBytes(20);
                    app._changeNumber = binaryReader.ReadUInt32();
                    app._properties = binaryReader.ReadPropertyTable();
                    string propertyValue = app._properties.GetPropertyValue<string>(null, new string[]
                    {
                        "appinfo",
                        "steam_edit",
                        "base_name"
                    });
                    if (propertyValue != null)
                    {
                        app._properties.SetPropertyValue(SteamAppPropertyType.String, propertyValue, new string[]
                        {
                            "appinfo",
                            "common",
                            "name"
                        });
                    }
                    string propertyValue2 = app._properties.GetPropertyValue<string>(null, new string[]
                    {
                        "appinfo",
                        "steam_edit",
                        "base_type"
                    });
                    if (propertyValue2 != null)
                    {
                        app._properties.SetPropertyValue(SteamAppPropertyType.String, propertyValue2, new string[]
                        {
                            "appinfo",
                            "common",
                            "type"
                        });
                    }
                }
                app._originalData = array;
                app.ClearCachedProps();
            }
            catch (Exception ex)
            {
                Log.Error(nameof(SteamApp), ex, string.Format("Failed to load entry with appId {0:X8}", app.ID));
            }
            return app;
        }
    }
}