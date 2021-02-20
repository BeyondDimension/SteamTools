using System.Application.UI.ViewModels;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace System.Application.Services
{
    /// <summary>
    /// 本地化、多语言服务
    /// </summary>
    public interface ILocalizationService
    {
        CultureInfo DefaultCurrentUICulture { get; }

        /// <summary>
        /// 更改语言
        /// </summary>
        /// <param name="cultureName"></param>
        void ChangeLanguage(string cultureName);

        /// <summary>
        /// 绑定资源字符串的视图模型组
        /// </summary>
        ILocalizationViewModel[] ViewModels { get; }

        public static IList<KeyValuePair<string, string>> Languages;

        static ILocalizationService()
        {
            Languages = new Dictionary<string, string>()
            {
                { "", "Auto" },
                { "en", "English" },
                { "zh-Hans", "Chinese(Simplified)" },
                { "zh-Hant", "Chinese(Traditional)" },
            }.ToList();
        }
    }
}