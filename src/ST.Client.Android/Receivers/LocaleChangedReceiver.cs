using Android.App;
using Android.Content;
using System.Application.UI.Resx;
using System.Globalization;
using System.Collections.Generic;

namespace System.Application.Receivers
{
    [BroadcastReceiver(Enabled = true, Exported = true)]
    [IntentFilter(new[] { Intent.ActionLocaleChanged }, Priority = int.MaxValue)]
    internal sealed class LocaleChangedReceiver : BroadcastReceiver
    {
        /// <summary>
        /// 当 Android 系统设置语言发生改变时
        /// </summary>
        /// <param name="context"></param>
        /// <param name="intent"></param>
        public override void OnReceive(Context? context, Intent? intent)
        {
            if (intent == null) return;
            if (intent.Action != Intent.ActionLocaleChanged) return;
            var cultureInfo = GetCultureInfo();
            if (cultureInfo == null) return;
            R.ChangeAutoLanguage(cultureInfo);
        }

        static CultureInfo? GetCultureInfo(string name)
        {
            CultureInfo? cultureInfo;
            try
            {
                cultureInfo = new(name);
            }
            catch
            {
                cultureInfo = null;
            }
            return cultureInfo;
        }

        static readonly IReadOnlyDictionary<string, string> icu_to_lcid = new Dictionary<string, string>()
        {
            { "gsw_", "de" },
            { "in_", "id" },
        };

        static CultureInfo? GetCultureInfo()
        {
            var locale = Java.Util.Locale.Default;
            var name = locale.ToString()!;
            CultureInfo? cultureInfo;
            if (!string.IsNullOrWhiteSpace(name))
            {
                foreach (var item in icu_to_lcid)
                {
                    if (name.StartsWith(item.Key))
                    {
                        cultureInfo = GetCultureInfo($"{item.Value}-{name.TrimStart(item.Key)}")
                            ?? GetCultureInfo(item.Value);
                        if (cultureInfo != null) return cultureInfo;
                    }
                }
                name = name!.Replace("_", "-");
                cultureInfo = GetCultureInfo(name);
                if (cultureInfo != null) return cultureInfo;
            }
            var lang = locale.Language;
            var c = locale.Country;
            if (!string.IsNullOrWhiteSpace(c))
            {
                cultureInfo = GetCultureInfo($"{lang}-{c}");
                if (cultureInfo != null) return cultureInfo;
            }
            var script = locale.Script;
            if (!string.IsNullOrWhiteSpace(script))
            {
                cultureInfo = GetCultureInfo($"{lang}-{script}");
                if (cultureInfo != null) return cultureInfo;
            }
            cultureInfo = GetCultureInfo(lang);
            return cultureInfo;
        }
    }
}