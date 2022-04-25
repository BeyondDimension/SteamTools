using System.Application.UI;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Versioning;
using System.Application.Models;

namespace System.Application.Settings
{
    partial class GameLibrarySettings
    {
        static readonly SerializableProperty<bool> _GameIsInstalledFilter = GetProperty(defaultValue: false, autoSave: true);

        /// <summary>
        /// 游戏类型筛选状态列表
        /// </summary>
        [SupportedOSPlatform("Windows7.0")]
        [SupportedOSPlatform("macOS")]
        [SupportedOSPlatform("Linux")]
        public static SerializableProperty<bool> GameIsInstalledFilter => _GameIsInstalledFilter;

        static readonly SerializableProperty<List<SteamAppType>>? _GameTypeFiltres = IApplication.IsDesktopPlatform ? GetProperty(defaultValue: new List<SteamAppType> { SteamAppType.Game, SteamAppType.Application, SteamAppType.Demo, SteamAppType.Beta }, autoSave: true) : null;

        /// <summary>
        /// 游戏类型筛选状态列表
        /// </summary>
        [SupportedOSPlatform("Windows7.0")]
        [SupportedOSPlatform("macOS")]
        [SupportedOSPlatform("Linux")]
        public static SerializableProperty<List<SteamAppType>> GameTypeFiltres => _GameTypeFiltres ?? throw new PlatformNotSupportedException();

        static readonly SerializableProperty<Dictionary<uint, string?>>? _HideGameList = IApplication.IsDesktopPlatform ? GetProperty(defaultValue: new Dictionary<uint, string?>(), autoSave: true) : null;

        /// <summary>
        /// 隐藏的游戏列表
        /// </summary>
        [SupportedOSPlatform("Windows7.0")]
        [SupportedOSPlatform("macOS")]
        [SupportedOSPlatform("Linux")]
        public static SerializableProperty<Dictionary<uint, string?>> HideGameList => _HideGameList ?? throw new PlatformNotSupportedException();

        static readonly SerializableProperty<Dictionary<uint, string?>?>? _AFKAppList = IApplication.IsDesktopPlatform ?
            GetProperty<Dictionary<uint, string?>?>(defaultValue: null, autoSave: true) : null;

        /// <summary>
        /// 挂时长游戏列表
        /// </summary>
        [SupportedOSPlatform("Windows7.0")]
        [SupportedOSPlatform("macOS")]
        [SupportedOSPlatform("Linux")]
        public static SerializableProperty<Dictionary<uint, string?>?> AFKAppList => _AFKAppList ?? throw new PlatformNotSupportedException();

        static readonly SerializableProperty<bool>? _IsAutoAFKApps
            = IApplication.IsDesktopPlatform ? GetProperty(defaultValue: true, autoSave: true) : null;

        /// <summary>
        /// 启用自动挂机
        /// </summary>
        [SupportedOSPlatform("Windows7.0")]
        [SupportedOSPlatform("macOS")]
        [SupportedOSPlatform("Linux")]
        public static SerializableProperty<bool> IsAutoAFKApps => _IsAutoAFKApps ?? throw new PlatformNotSupportedException();

        // [SupportedOSPlatform("Windows7.0")]
        // [SupportedOSPlatform("macOS")]
        // [SupportedOSPlatform("Linux")]
        // public static readonly Lazy<ReadOnlyCollection<uint>>? DefaultIgnoreList = IApplication.IsDesktopPlatform ? new(new ReadOnlyCollection<uint>(new uint[]
        // {
        //     5,
        //     7,
        //     8,
        //     90,
        //     92,
        //     205,
        //     211,
        //     215,
        //     218,
        //     310,
        //     364,
        //     459,
        //     480,
        //     513,
        //     563,
        //     564,
        //     575,
        //     629,
        //     635,
        //     640,
        //     644,
        //     740,
        //     745,
        //     753,
        //     760,
        //     761,
        //     764,
        //     765,
        //     766,
        //     767,
        //     1007,
        //     1213,
        //     1255,
        //     1260,
        //     1273,
        //     2145,
        //     2403,
        //     4270,
        //     4940,
        //     8680,
        //     8710,
        //     8730,
        //     8770,
        //     12250,
        //     12750,
        //     13180,
        //     13260,
        //     16830,
        //     16864,
        //     16865,
        //     16871,
        //     16872,
        //     16879,
        //     17505,
        //     17515,
        //     17525,
        //     17535,
        //     17555,
        //     17575,
        //     17585,
        //     17705,
        //     61800,
        //     61810,
        //     61820,
        //     61830,
        //     104700,
        //     201700,
        //     224220,
        //     232610,
        //     375350,
        //     245550,
        //     254000,
        //     254020,
        //     254040,
        //     259280,
        //     285050,
        //     312710,
        //     314700,
        //     321040,
        //     329950,
        //     331710,
        //     344750,
        //     354850,
        //     368900,
        //     373110,
        //     383030,
        //     385200,
        //     388870,
        //     392050,
        //     392870,
        //     395980,
        //     397620,
        //     399580,
        //     407730,
        //     407740,
        //     407750,
        //     410590,
        //     410700,
        //     416270,
        //     428430,
        //     431270,
        //     432150,
        //     447880,
        //     449630,
        //     457930,
        //     457940,
        //     460660,
        //     461570,
        //     462370,
        //     463620,
        //     473580,
        //     473620,
        //     473630,
        //     473640,
        //     473650,
        //     486340,
        //     488200,
        //     491250,
        //     492240,
        //     496600,
        //     501690,
        //     505440,
        //     509840,
        //     516230,
        //     516700,
        //     524440,
        //     526790,
        //     528210,
        //     533710,
        //     545950,
        //     546450,
        //     559500,
        //     559940,
        //     562020,
        //     576080,
        //     576440,
        //     581620,
        //     592490,
        //     603770,
        //     603780,
        //     605470,
        //     654310,
        //     700580
        //})) : null;
    }
}
