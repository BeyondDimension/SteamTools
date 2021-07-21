using Android.App;
using Android.Content;
using System.Application.UI.Resx;

namespace System.Application.Receivers
{
    [BroadcastReceiver(Enabled = true, Exported = true)]
    [IntentFilter(new[] { Intent.ActionLocaleChanged }, Priority = int.MaxValue)]
    internal sealed class LocaleChangedReceiver : BroadcastReceiver
    {
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
            R.ChangeAutoLanguage(new(name));
        }
    }
}