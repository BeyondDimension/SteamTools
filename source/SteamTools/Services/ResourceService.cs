using Livet;
using SteamTools.Models.Settings;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using SteamTools.Properties;

namespace SteamTools.Services
{
    /// <summary>
    /// 提供对多语言资源的访问。
    /// </summary>
    public class ResourceService : NotificationObject
    {
        // singleton
        public static ResourceService Current { get; } = new ResourceService();

        /// <summary>
        /// 支持的语言简写
        /// </summary>
        private readonly string[] supportedCultureNames =
        {
            "zh-Hans", // Resources.resx
            "zh-Hant",
            "en",
        };

        /// <summary>
        /// 支持的语言Steam平台名称
        /// </summary>
        private readonly Dictionary<string, string> supportedCultureSteamNames = new Dictionary<string, string>
        {
            { "zh-Hans","schinese"},
            { "zh-Hant","tchinese"},
            { "en","english"},
        };

        /// <summary>
        /// 获取多语种资源。
        /// </summary>
        public SteamTools.Properties.Resources Resources { get; }

        /// <summary>
        /// 获得受支持的文化。
        /// </summary>
        public IReadOnlyCollection<CultureInfo> SupportedCultures { get; }

        private ResourceService()
        {
            this.Resources = new SteamTools.Properties.Resources();
            this.SupportedCultures = this.supportedCultureNames
                .Select(x =>
                {
                    try
                    {
                        return CultureInfo.GetCultureInfo(x);
                    }
                    catch (CultureNotFoundException)
                    {
                        return null;
                    }
                })
                .Where(x => x != null)
                .ToList();
            SteamTools.Properties.Resources.Culture = this.SupportedCultures.First();
        }

        /// <summary>
        /// 使用指定的区域性名称更改资源的区域性。
        /// </summary>
        /// <param name="name"></param>
        public void ChangeCulture(string name)
        {
            Resources.Culture = this.SupportedCultures.SingleOrDefault(x => x.Name == name);
            GeneralSettings.Culture.Value = SteamTools.Properties.Resources.Culture?.Name;
            //Debug.WriteLine(SteamTools.Properties.Resources.ResourceManager.GetString("Welcome", Resources.Culture));
            this.RaisePropertyChanged(nameof(this.Resources));
        }

        public string GetCurrentCultureSteamLanguageName()
        {
            return supportedCultureSteamNames[SteamTools.Properties.Resources.Culture.Name];
        }
    }
}
