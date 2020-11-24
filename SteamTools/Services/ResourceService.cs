using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace SteamTools.Services
{
    /// <summary>
    /// 多言語化されたリソースへのアクセスを提供します。
    /// </summary>
    public class ResourceService : INotifyPropertyChanged
    {
        // singleton
        public static ResourceService Current { get; } = new ResourceService();

        /// <summary>
        /// 支持的语言简写
        /// </summary>
        private readonly string[] supportedCultureNames =
        {
            "zh-CN", // Resources.resx
            "zh-TW",
            "en",
        };

        /// <summary>
        /// 支持的语言Steam平台名称
        /// </summary>
        private readonly Dictionary<string, string> supportedCultureSteamNames = new Dictionary<string, string>
        {
            { "zh-CN","schinese"},
            { "zh-TW","tchinese"},
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
            SteamTools.Properties.Resources.Culture = this.SupportedCultures.SingleOrDefault(x => x.Name == name);
            this.OnPropertyChanged(nameof(this.Resources));
        }

        public string GetCurrentCultureSteamLanguageName() 
        {
            return supportedCultureSteamNames[SteamTools.Properties.Resources.Culture.Name];
        }

        #region PropertyChanged event

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion
    }
}
