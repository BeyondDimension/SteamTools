using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using ReactiveUI;

namespace System.Application.Resx
{
    /// <summary>
    /// 提供对多语言资源的访问。
    /// </summary>
    public class ResourceService : ReactiveObject
    {
        // singleton
        public static ResourceService Current => new ResourceService();

        /// <summary>
        /// 支持的语言简写
        /// </summary>
        private readonly string[] supportedCultureNames =
        {
            "zh-Hans", // Resources.resx
            "zh-Hant",
            "en",
            "ja",
            "ru",
        };

        /// <summary>
        /// 支持的语言Steam平台名称
        /// </summary>
        private readonly Dictionary<string, string> supportedCultureSteamNames = new Dictionary<string, string>
        {
            { "zh-Hans","schinese"},
            { "zh-Hant","tchinese"},
            { "en","english"},
            { "ko","koreana"},
            { "ja","japanese"},
            { "ru","russian"},
        };

        /// <summary>
        /// 获取多语种资源。
        /// </summary>
        public System.Application.UI.Resx.AppResources Resources { get; }

        /// <summary>
        /// 获得受支持的文化。
        /// </summary>
        public IReadOnlyCollection<CultureInfo> SupportedCultures { get; }

        private ResourceService()
        {
            this.Resources = new System.Application.UI.Resx.AppResources();
#pragma warning disable CS8619 // 值中的引用类型的为 Null 性与目标类型不匹配。
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
#pragma warning restore CS8619 // 值中的引用类型的为 Null 性与目标类型不匹配。
            System.Application.UI.Resx.AppResources.Culture = this.SupportedCultures.First();
        }

        /// <summary>
        /// 使用指定的区域性名称更改资源的区域性。
        /// </summary>
        /// <param name="name"></param>
        public void ChangeCulture(string name)
        {
            System.Application.UI.Resx.AppResources.Culture = this.SupportedCultures.SingleOrDefault(x => x.Name == name);
            //GeneralSettings.Culture.Value = SteamTools.Properties.Resources.Culture?.Name;
            this.RaisePropertyChanged(nameof(this.Resources));
        }

        public string GetCurrentCultureSteamLanguageName()
        {
            return System.Application.UI.Resx.AppResources.Culture == null ? supportedCultureSteamNames.First().Value : supportedCultureSteamNames[System.Application.UI.Resx.AppResources.Culture.Name];
        }
    }
}
