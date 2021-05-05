using DynamicData;
using DynamicData.Binding;
using Newtonsoft.Json.Linq;
using ReactiveUI;
using System.Application.Models;
using System.Application.Services;
using System.Application.UI.Resx;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Properties;
using System.Reactive.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace System.Application.UI.ViewModels
{
    public class AchievementWindowViewModel : WindowViewModel
    {
        private int AppId { get; }

        #region 成就列表
        private readonly ReadOnlyObservableCollection<AchievementInfo>? _Achievements;
        private readonly SourceList<AchievementInfo> _AchievementsSourceList;
        //private IReadOnlyCollection<AchievementInfo>? _BackAchievements;
        public ReadOnlyObservableCollection<AchievementInfo>? Achievements => _Achievements;
        #endregion

        #region 统计列表

        private readonly ReadOnlyObservableCollection<StatInfo>? _Statistics;
        private readonly SourceList<StatInfo> _StatisticsSourceList;
        public ReadOnlyObservableCollection<StatInfo>? Statistics => _Statistics;
        #endregion

        private bool? _IsCheckAll = false;
        public bool? IsCheckAll
        {
            get => _IsCheckAll;
            set => this.RaiseAndSetIfChanged(ref _IsCheckAll, value);
        }

        private string? _SearchText;
        public string? SearchText
        {
            get => _SearchText;
            set => this.RaiseAndSetIfChanged(ref _SearchText, value);
        }

        Func<AchievementInfo, bool> PredicateAchievementName(string? text)
        {
            return s =>
            {
                if (s == null || s.Name == null || s.Description == null)
                    return false;
                if (string.IsNullOrEmpty(text))
                    return true;
                if (s.Name.Contains(text, StringComparison.OrdinalIgnoreCase) ||
                       s.Description.ToString().Contains(text, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }

                return false;
            };
        }

        Func<StatInfo, bool> PredicateStatInfoName(string? text)
        {
            return s =>
            {
                if (s == null || s.DisplayName == null || s.Id == null)
                    return false;
                if (string.IsNullOrEmpty(text))
                    return true;
                if (s.DisplayName.Contains(text, StringComparison.OrdinalIgnoreCase) ||
                       s.Id.ToString().Contains(text, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }

                return false;
            };
        }

        public AchievementWindowViewModel(int appid) : base()
        {
            Title = ThisAssembly.AssemblyTrademark + " | " + AppResources.AchievementManage;

            SteamConnectService.Current.Initialize(appid);

            if (SteamConnectService.Current.IsConnectToSteam == false)
            {
                MessageBoxCompat.ShowAsync(AppResources.Achievement_Warning_1, Title, MessageBoxButtonCompat.OKCancel).ContinueWith(s =>
                 {
                     EnforceClose();
                 }).Wait();
            }

            _AchievementsSourceList = new SourceList<AchievementInfo>();
            _StatisticsSourceList = new SourceList<StatInfo>();

            var nameAchievementsFilter = this.WhenAnyValue(x => x.SearchText).Select(PredicateAchievementName);

            var nameStatisticsFilter = this.WhenAnyValue(x => x.SearchText).Select(PredicateStatInfoName);

            _AchievementsSourceList
                .Connect()
                .Filter(nameAchievementsFilter)
                .ObserveOn(RxApp.MainThreadScheduler)
                .Sort(SortExpressionComparer<AchievementInfo>.Descending(x => x.IsAchieved).ThenByAscending(x => x.Name))
                .Bind(out _Achievements)
                .Subscribe();


            _StatisticsSourceList
                .Connect()
                .Filter(nameStatisticsFilter)
                .ObserveOn(RxApp.MainThreadScheduler)
                .Sort(SortExpressionComparer<StatInfo>.Descending(x => x.Id))
                .Bind(out _Statistics)
                .Subscribe();

            this.WhenAnyValue(x => x.IsCheckAll)
                .Subscribe(x =>
                {
                    foreach (var item in _AchievementsSourceList.Items)
                        item.IsChecked = x == true;
                });

            AppId = appid;
            string name = ISteamworksLocalApiService.Instance.GetAppData((uint)appid, "name");
            name ??= appid.ToString();
            Title = ThisAssembly.AssemblyTrademark + " | " + name;
            ToastService.Current.Set(AppResources.Achievement_LoadData);

            ISteamworksLocalApiService.Instance.AddUserStatsReceivedCallback((param) =>
            {
                if (param.Result != 1)
                {
                    MessageBoxCompat.ShowAsync(AppResources.Achievement_Warning_2.Format(param.Result), Title, MessageBoxButtonCompat.OKCancel).ContinueWith(s =>
                    {
                        EnforceClose();
                    }).Wait();
                }
                if (this.LoadUserGameStatsSchema() == false)
                {
                    MessageBoxCompat.ShowAsync($"Failed to load schema", Title, MessageBoxButtonCompat.OKCancel).ContinueWith(s =>
                    {
                        EnforceClose();
                    }).Wait();
                }
                GetAchievements();
                GetStatistics();

                //ToastService.Current.Notify($"获取到 {this._AchievementsSourceList.Count} 个成就和 {this._StatisticsSourceList.Count} 条统计信息");
                ToastService.Current.Notify(AppResources.Achievement_LoadSucces.Format(_AchievementsSourceList.Count, _StatisticsSourceList.Count));
            });

            Task.Run(() =>
            {
                while (true)
                {
                    ISteamworksLocalApiService.Instance.RunCallbacks(false);
                    Threading.Thread.Sleep(2000);
                }
            }).Forget();

            this.RefreshStats_Click();
        }



        private void EnforceClose()
        {
            Process.GetCurrentProcess().Kill();
        }

        private bool LoadUserGameStatsSchema()
        {
            var path = ISteamService.Instance.SteamDirPath;
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

            var currentLanguage = R.GetCurrentCultureSteamLanguageName(); ;
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

                            this._StatisticsSourceList.Add(new IntStatInfo()
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

                            this._StatisticsSourceList.Add(new FloatStatInfo()
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

                                        this._AchievementsSourceList.Add(new AchievementInfo()
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

            language = ISteamworksLocalApiService.Instance.GetCurrentGameLanguage();

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
            foreach (var def in this._AchievementsSourceList.Items)
            {
                if (string.IsNullOrEmpty(def.Id) == true)
                {
                    continue;
                }

                if (ISteamworksLocalApiService.Instance.GetAchievementAndUnlockTime(def.Id, out bool isAchieved, out var unlockTime) == false)
                {
                    continue;
                }

                //ISteamworksLocalApiService.Instance.GetAchievementAchievedPercent(def.Id, out float percent);

                var info = new AchievementInfo()
                {
                    AppId = def.AppId,
                    Id = def.Id,
                    IsAchieved = isAchieved,
                    IsChecked = isAchieved,
                    IconNormal = string.IsNullOrEmpty(def.IconNormal) ? null : def.IconNormal,
                    IconLocked = string.IsNullOrEmpty(def.IconLocked) ? def.IconNormal : def.IconLocked,
                    Permission = def.Permission,
                    UnlockTime = unlockTime.ToDateTimeS(),
                    UnlockTimeUnix = unlockTime,
                    Name = def.Name,
                    Description = def.Description,
                    //Percent = percent,
                };

                list.Add(info);
            }
            //this.Achievements = new ObservableCollection<AchievementInfo>(list.OrderByDescending(s => s.IsAchieved).ThenBy(s => s.Id).ToList());
            //Parallel.ForEach(list, async achievement =>
            //{
            //    achievement.IconStream = await IHttpService.Instance.GetImageAsync(achievement.IconUrl, ImageChannelType.SteamAchievementIcon);
            //});
            _AchievementsSourceList.Clear();
            this._AchievementsSourceList.AddRange(list);
            //_BackAchievements = Achievements;
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
                if (ISteamworksLocalApiService.Instance.SetAchievement(info.Id, info.IsAchieved) == false)
                {
                    MessageBoxCompat.ShowAsync(AppResources.Achievement_Warning_3.Format(info.Name), Title, MessageBoxButtonCompat.OK).ContinueWith(s =>
                    {
                        EnforceClose();
                    });
                    return -1;
                }
            }

            return achievements.Count;
        }

        private void GetStatistics()
        {
            var list = new List<StatInfo>();
            foreach (var rdef in this._StatisticsSourceList.Items)
            {
                if (string.IsNullOrEmpty(rdef.Id) == true)
                {
                    continue;
                }
                if (rdef is IntStatInfo idef)
                {
                    if (ISteamworksLocalApiService.Instance.GetStatValue(idef.Id, out int value))
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
                    if (ISteamworksLocalApiService.Instance.GetStatValue(fdef.Id, out float value))
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
            //this.Statistics = new ObservableCollection<StatInfo>(list);

            _StatisticsSourceList.Clear();
            _StatisticsSourceList.AddRange(list);
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
                    if (ISteamworksLocalApiService.Instance.SetStatValue(
                        intStat.Id,
                        intStat.IntValue) == false)
                    {
                        Toast.Show(AppResources.Achievement_Warning_4.Format(intStat.Id));
                        return -1;
                    }
                }
                else if (stat is FloatStatInfo floatStat)
                {
                    if (ISteamworksLocalApiService.Instance.SetStatValue(
                        floatStat.Id,
                        floatStat.FloatValue) == false)
                    {
                        Toast.Show(AppResources.Achievement_Warning_4.Format(floatStat.Id));
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
            this._AchievementsSourceList.Clear();
            this._StatisticsSourceList.Clear();
            //Achievements = new ObservableCollection<AchievementInfo>();
            //Statistics = new ObservableCollection<StatInfo>();
            ISteamworksLocalApiService.Instance.RequestCurrentStats();
            //ISteamworksLocalApiService.Instance.RequestGlobalAchievementPercentages();
            //return r;
        }

        public async void ResetAllStats_Click()
        {
            var result = await MessageBoxCompat.ShowAsync(AppResources.Achievement_ResetWaring_1, Title, MessageBoxButtonCompat.OKCancel);
            if (result == MessageBoxResultCompat.OK)
            {
                var achievementsToo = await MessageBoxCompat.ShowAsync(AppResources.Achievement_ResetWaring_2, Title, MessageBoxButtonCompat.OKCancel);


                var real = await MessageBoxCompat.ShowAsync(AppResources.Achievement_ResetWaring_3, Title, MessageBoxButtonCompat.OKCancel);
                if (real == MessageBoxResultCompat.OK)
                {
                    if (ISteamworksLocalApiService.Instance.ResetAllStats(achievementsToo == MessageBoxResultCompat.OK))
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

            if (ISteamworksLocalApiService.Instance.StoreStats() == false)
            {
                this.ResetAllStats_Click();
                return;
            }
            ToastService.Current.Notify(AppResources.Achievement_EditSucces.Format(achievements, stats));
            this.RefreshStats_Click();
        }
    }
}