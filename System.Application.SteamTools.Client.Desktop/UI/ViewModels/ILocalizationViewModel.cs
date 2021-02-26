using System.Application.UI.Resx;
using System.Linq;
using System.Reflection;

namespace System.Application.UI.ViewModels
{
    /// <summary>
    /// 本地化、多语言视图模型
    /// </summary>
    [Obsolete("Languages")]
    public interface ILocalizationViewModel
    {
        public void OnChangeLanguage()
        {
            var properties = GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance);
            var query = from p in properties
                        let attr = p.GetCustomAttribute<ResStringAttribute>()
                        where p.PropertyType == typeof(string) && attr != null
                        select (p, attr.ResId);
            foreach (var (p, ResId) in query)
            {
                var resId = ResId ?? p.Name;
                var value = AppResources.ResourceManager.GetString(resId);
                p.SetValue(this, value);
            }
        }
    }
}