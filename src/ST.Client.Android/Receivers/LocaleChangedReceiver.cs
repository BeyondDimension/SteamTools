using Android.App;
using Android.Content;
using System.Application.UI.Resx;
using System.Globalization;

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
            var locale = Java.Util.Locale.Default;
            string name;
            if (!string.IsNullOrWhiteSpace(locale.Script))
            {
                name = $"{locale.Language}-{locale.Script}";
            }
            else
            {
                name = locale.Language;
            }
            CultureInfo? cultureInfo;
            try
            {
                cultureInfo = new(name);
            }
            catch
            {
                cultureInfo = null;
            }
            if (cultureInfo == null) return;
            R.ChangeAutoLanguage(cultureInfo);
        }
    }
}