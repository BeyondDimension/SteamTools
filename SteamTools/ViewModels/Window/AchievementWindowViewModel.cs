using MetroTrilithon.Mvvm;
using SAM.API.Types;
using SteamTool.Core;
using SteamTool.Core.Common;
using SteamTool.Model;
using SteamTool.Steam.Service;
using SteamTools.Properties;
using SteamTools.Services;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace SteamTools.ViewModels
{
    public class AchievementWindowViewModel : MainWindowViewModelBase
    {
        private readonly SteamToolService toolService = SteamToolCore.Instance.Get<SteamToolService>();

        private int AppId { get; }

        #region 成就列表
        private IList<AchievementInfo> _BackAchievements;
        private IList<AchievementInfo> _Achievements;
        public IList<AchievementInfo> Achievements
        {
            get { return _Achievements; }
            set
            {
                if (this._Achievements != value)
                {
                    this._Achievements = value;
                    this.RaisePropertyChanged();
                }
            }
        }
        #endregion

        #region 统计列表
        private IList<StatInfo> _Statistics;
        public IList<StatInfo> Statistics
        {
            get { return _Statistics; }
            set
            {
                if (this._Statistics != value)
                {
                    this._Statistics = value;
                    this.RaisePropertyChanged();
                }
            }
        }
        #endregion

        private bool _CheckAll;
        public bool CheckAll
        {
            get { return _CheckAll; }
            set
            {
                if (this._CheckAll != value)
                {
                    this._CheckAll = value;
                    var achievements = this.Achievements.ToArray();
                    foreach (var item in achievements)
                    {
                        item.IsChecked = value;
                    }
                    this.Achievements = achievements.ToList();
                    this.RaisePropertyChanged();
                }
            }
        }

        private string _Text;
        public string Text
        {
            get { return _Text; }
            set
            {
                if (this._Text != value)
                {
                    this._Text = value;
                    if (!string.IsNullOrEmpty(value))
                    {
                        this.Achievements = Achievements.Where(w => w.Name.IndexOf(Text, StringComparison.OrdinalIgnoreCase) > -1).ToList();
                    }
                    else
                    {
                        Achievements = _BackAchievements;
                    }
                    this.RaisePropertyChanged();
                }
            }
        }

        public AchievementWindowViewModel(bool isMainWindow, int appid) : base(isMainWindow)
        {
            SteamConnectService.Current.Initialize(appid);
            if (SteamConnectService.Current.IsConnectToSteam == false)
            {
                MessageBox.Show("与Steam建立连接失败，可能是该游戏没有成就，或者你没有该游戏。");
                EnforceClose();
            }

            Achievements = new List<AchievementInfo>();
            Statistics = new List<StatInfo>();
            AppId = appid;
            string name = SteamConnectService.Current.ApiService.GetAppData((uint)appid, "name");
            name ??= appid.ToString();
            Title = Resources.WinTitle + " | " + name;
            StatusService.Current.Set("Loading...");
            SteamConnectService.Current.ApiService.AddUserStatsReceivedCallback((param) =>
            {
                if (param.Result != 1)
                {
                    this.Dialog($"错误代码: {param.Result}\r\n检索成就统计信息时出错，可能是该游戏没有成就，或者你没有该游戏。");
                    EnforceClose();
                }
                if (this.LoadUserGameStatsSchema() == false)
                {
                    this.Dialog($"Failed to load schema");
                    EnforceClose();
                }
                GetAchievements();
                GetStatistics();
                StatusService.Current.Set($"获取到 {this.Achievements.Count} 个成就和 {this.Statistics.Count} 条统计信息");
            });

            Task.Run(async () =>
            {
                while (true)
                {
                    SteamConnectService.Current.ApiService.RunCallbacks(false);
                    await Task.Delay(1500);
                }
            }).ContinueWith(s => { Logger.Error(s.Exception); WindowService.Current.ShowDialogWindow(s.Exception.Message); }, TaskContinuationOptions.OnlyOnFaulted);

            this.RefreshStats_Click();
            //this.Initialize();
        }
        private void EnforceClose()
        {
            Process.GetCurrentProcess().Kill();
        }

        private bool LoadUserGameStatsSchema()
        {
            var path = toolService.SteamPath;
            path = Path.Combine(path, "appcache");
            path = Path.Combine(path, "stats");
            path = Path.Combine(path, string.Format(
                CultureInfo.InvariantCulture,
                "UserGameStatsSchema_{0}.bin",
                this.AppId));

            if (File.Exists(path) == false)
            {
                return false;
            }

            var kv = KeyValue.LoadAsBinary(path);

            if (kv == null)
            {
                return false;
            }

            //var currentLanguage = SteamConnectService.Current.ApiService.GetCurrentGameLanguage();
            var currentLanguage = ResourceService.Current.GetCurrentCultureSteamLanguageName(); ;
            var stats = kv[this.AppId.ToString(CultureInfo.InvariantCulture)]["stats"];
            if (stats.Valid == false ||
                stats.Children == null)
            {
                return false;
            }

            foreach (var stat in stats.Children)
            {
                if (stat.Valid == false)
                {
                    continue;
                }

                var rawType = stat["type_int"].Valid
                                  ? stat["type_int"].AsInteger(0)
                                  : stat["type"].AsInteger(0);
                var type = (UserStatType)rawType;
                switch (type)
                {
                    case UserStatType.Invalid:
                        {
                            break;
                        }

                    case UserStatType.Integer:
                        {
                            var id = stat["name"].AsString("");
                            string name = GetLocalizedString(stat["display"]["name"], currentLanguage, id);

                            this._Statistics.Add(new IntStatInfo()
                            {
                                Id = stat["name"].AsString(""),
                                DisplayName = name,
                                MinValue = stat["min"].AsInteger(int.MinValue),
                                MaxValue = stat["max"].AsInteger(int.MaxValue),
                                MaxChange = stat["maxchange"].AsInteger(0),
                                IncrementOnly = stat["incrementonly"].AsBoolean(false),
                                DefaultValue = stat["default"].AsInteger(0),
                                Permission = stat["permission"].AsInteger(0),
                            });
                            break;
                        }

                    case UserStatType.Float:
                    case UserStatType.AverageRate:
                        {
                            var id = stat["name"].AsString("");
                            string name = GetLocalizedString(stat["display"]["name"], currentLanguage, id);

                            this._Statistics.Add(new FloatStatInfo()
                            {
                                Id = stat["name"].AsString(""),
                                DisplayName = name,
                                MinValue = stat["min"].AsFloat(float.MinValue),
                                MaxValue = stat["max"].AsFloat(float.MaxValue),
                                MaxChange = stat["maxchange"].AsFloat(0.0f),
                                IncrementOnly = stat["incrementonly"].AsBoolean(false),
                                DefaultValue = stat["default"].AsFloat(0.0f),
                                Permission = stat["permission"].AsInteger(0),
                            });
                            break;
                        }

                    case UserStatType.Achievements:
                    case UserStatType.GroupAchievements:
                        {
                            if (stat.Children != null)
                            {
                                foreach (var bits in stat.Children.Where(
                                    b => string.Compare(b.Name, "bits", StringComparison.InvariantCultureIgnoreCase) == 0))
                                {
                                    if (bits.Valid == false ||
                                        bits.Children == null)
                                    {
                                        continue;
                                    }

                                    foreach (var bit in bits.Children)
                                    {
                                        string id = bit["name"].AsString("");
                                        string name = GetLocalizedString(bit["display"]["name"], currentLanguage, id);
                                        string desc = GetLocalizedString(bit["display"]["desc"], currentLanguage, "");

                                        this._Achievements.Add(new AchievementInfo()
                                        {
                                            AppId = AppId,
                                            Id = id,
                                            Name = name,
                                            Description = desc,
                                            IconNormal = bit["display"]["icon"].AsString(""),
                                            IconLocked = bit["display"]["icon_gray"].AsString(""),
                                            IsHidden = bit["display"]["hidden"].AsBoolean(false),
                                            Permission = bit["permission"].AsInteger(0),
                                        });
                                    }
                                }
                            }

                            break;
                        }

                    default:
                        {
                            throw new InvalidOperationException("invalid stat type");
                        }
                }
            }

            return true;
        }

        private static string GetLocalizedString(KeyValue kv, string language, string defaultValue)
        {
            var name = kv[language].AsString("");
            if (string.IsNullOrEmpty(name) == false)
            {
                return name;
            }

            language = SteamConnectService.Current.ApiService.GetCurrentGameLanguage();

            name = kv[language].AsString("");
            if (string.IsNullOrEmpty(name) == false)
            {
                return name;
            }

            if (language != "english")
            {
                name = kv["english"].AsString("");
                if (string.IsNullOrEmpty(name) == false)
                {
                    return name;
                }
            }

            name = kv.AsString("");
            if (string.IsNullOrEmpty(name) == false)
            {
                return name;
            }

            return defaultValue;
        }

        private void GetAchievements()
        {
            var list = new List<AchievementInfo>();
            foreach (var def in this.Achievements)
            {
                if (string.IsNullOrEmpty(def.Id) == true)
                {
                    continue;
                }

                if (SteamConnectService.Current.ApiService.GetAchievementAndUnlockTime(def.Id, out bool isAchieved, out var unlockTime) == false)
                {
                    continue;
                }

                var info = new AchievementInfo()
                {
                    AppId = def.AppId,
                    Id = def.Id,
                    IsAchieved = isAchieved,
                    IsChecked = isAchieved,
                    IconNormal = string.IsNullOrEmpty(def.IconNormal) ? null : def.IconNormal,
                    IconLocked = string.IsNullOrEmpty(def.IconLocked) ? def.IconNormal : def.IconLocked,
                    Permission = def.Permission,
                    UnlockTime = new DateTime(1970, 1, 1, 8, 0, 0).AddSeconds(unlockTime),
                    UnlockTimeUnix = unlockTime,
                    Name = def.Name,
                    Description = def.Description,
                };
                //if (SteamConnectService.Current.ApiService.GetAchievementAchievedPercent(def.Id, out float percent) == true)
                //{
                //    info.Percent = percent;
                //}
                list.Add(info);
            }
            this.Achievements = list.OrderByDescending(s => s.IsAchieved).ThenBy(s => s.Id).ToList();
            _BackAchievements = Achievements;
        }

        private int StoreAchievements()
        {
            if (this.Achievements.Count == 0)
            {
                return 0;
            }

            var achievements = new List<AchievementInfo>();
            foreach (var achievementInfo in this.Achievements)
            {
                if (achievementInfo != null &&
                    achievementInfo.IsAchieved != achievementInfo.IsChecked)
                {
                    achievementInfo.IsAchieved = achievementInfo.IsChecked;
                    achievements.Add(achievementInfo);
                }
            }

            if (achievements.Count == 0)
            {
                return 0;
            }

            foreach (AchievementInfo info in achievements)
            {
                if (SteamConnectService.Current.ApiService.SetAchievement(info.Id, info.IsAchieved) == false)
                {
                    this.Dialog(string.Format(CultureInfo.CurrentCulture, "修改成就{0}时发生错误", info.Name), "Error");
                    return -1;
                }
            }

            return achievements.Count;
        }

        private void GetStatistics()
        {
            var list = new List<StatInfo>();
            foreach (var rdef in this.Statistics)
            {
                if (string.IsNullOrEmpty(rdef.Id) == true)
                {
                    continue;
                }
                if (rdef is IntStatInfo idef)
                {
                    if (SteamConnectService.Current.ApiService.GetStatValue(idef.Id, out int value))
                    {
                        list.Add(new IntStatInfo()
                        {
                            Id = idef.Id,
                            DisplayName = idef.DisplayName,
                            IntValue = value,
                            OriginalValue = value,
                            IsIncrementOnly = idef.IncrementOnly,
                            Permission = idef.Permission,
                        });
                    }
                }
                else if (rdef is FloatStatInfo fdef)
                {
                    if (SteamConnectService.Current.ApiService.GetStatValue(fdef.Id, out float value))
                    {
                        list.Add(new FloatStatInfo()
                        {
                            Id = fdef.Id,
                            DisplayName = fdef.DisplayName,
                            FloatValue = value,
                            OriginalValue = value,
                            IsIncrementOnly = fdef.IncrementOnly,
                            Permission = fdef.Permission,
                        });
                    }
                }
            }
            this.Statistics = list;
        }

        private int StoreStatistics()
        {
            if (this.Statistics.Count == 0)
            {
                return 0;
            }

            var statistics = this.Statistics.Where(stat => stat.IsModified).ToList();
            if (statistics.Count == 0)
            {
                return 0;
            }

            foreach (var stat in statistics)
            {
                if (stat is IntStatInfo intStat)
                {
                    if (SteamConnectService.Current.ApiService.SetStatValue(
                        intStat.Id,
                        intStat.IntValue) == false)
                    {
                        this.Dialog(string.Format(CultureInfo.CurrentCulture, "修改统计{0}时发生错误", intStat.Id), "Error");
                        return -1;
                    }
                }
                else if (stat is FloatStatInfo floatStat)
                {
                    if (SteamConnectService.Current.ApiService.SetStatValue(
                        floatStat.Id,
                        floatStat.FloatValue) == false)
                    {
                        this.Dialog(string.Format(CultureInfo.CurrentCulture, "修改统计{0}时发生错误", floatStat.Id), "Error");
                        return -1;
                    }
                }
                else
                {
                    throw new InvalidOperationException("unsupported stat type");
                }
            }

            return statistics.Count;
        }

        public void RefreshStats_Click()
        {
            Achievements = new List<AchievementInfo>();
            Statistics = new List<StatInfo>();
            SteamConnectService.Current.ApiService.RequestCurrentStats();
            //SteamConnectService.Current.ApiService.RequestGlobalAchievementPercentages();
            //return r;
        }

        public void ResetAllStats_Click()
        {
            //WindowService.Current.GetDialogWindow("测试").ShowDialog();
            if (this.Dialog("确定要复位统计信息吗?"))
            {
                var achievementsToo = this.Dialog("也要复位成就信息吗?");
                if (this.Dialog("该操作不是还原当前修改，而是使所有成就和统计数据归零。\r\n真的确定吗?"))
                {
                    if (SteamConnectService.Current.ApiService.ResetAllStats(achievementsToo) == false)
                    {
                        return;
                    }
                    this.RefreshStats_Click();
                }
            }

        }

        public void SaveChange_Click()
        {
            int achievements = this.StoreAchievements();
            if (achievements < 0)
            {
                this.RefreshStats_Click();
                return;
            }

            int stats = this.StoreStatistics();
            if (stats < 0)
            {
                this.RefreshStats_Click();
                return;
            }

            if (SteamConnectService.Current.ApiService.StoreStats() == false)
            {
                this.ResetAllStats_Click();
                return;
            }
            var text = $"修改了 {achievements} 个成就和 {stats} 条统计信息";
            StatusService.Current.Notify(text);
            //this.Dialog($"修改了 {achievements} 个成就和 {stats} 统计信息");
            this.RefreshStats_Click();
        }
    }
}
