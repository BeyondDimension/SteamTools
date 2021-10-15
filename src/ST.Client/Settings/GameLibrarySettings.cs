using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using System.Threading.Tasks;

namespace System.Application.Settings
{
    public sealed partial class GameLibrarySettings : SettingsHost2<GameLibrarySettings>
    {
        /// <summary>
        /// 隐藏的游戏列表
        /// </summary>
        public static SerializableProperty<Dictionary<uint, string?>> HideGameList { get; }
            = GetProperty(defaultValue: new Dictionary<uint, string?>(), autoSave: true);

        /// <summary>
        /// 挂时长游戏列表
        /// </summary>
        public static SerializableProperty<Dictionary<uint, string?>> AFKAppList { get; }
            = GetProperty(defaultValue: new Dictionary<uint, string?>(), autoSave: true);

        /// <summary>
        /// 启用自动挂机
        /// </summary>
        public static SerializableProperty<bool> IsAutoAFKApps { get; }
            = GetProperty(defaultValue: true, autoSave: true);
    }
}