using Android.Content.PM;

// ReSharper disable once CheckNamespace
namespace System.Application
{
    public static class ManifestConstants
    {
        public const ConfigChanges ConfigurationChanges =
            ConfigChanges.ScreenSize |
            ConfigChanges.Orientation |
            ConfigChanges.Keyboard |
            ConfigChanges.KeyboardHidden |
            ConfigChanges.ScreenLayout |
            ConfigChanges.SmallestScreenSize;

        public const string MainTheme = "@style/MainTheme";
        public const string MainTheme_NoActionBar = "@style/MainTheme.NoActionBar";
        public const string MainTheme_Splash = "@style/MainTheme.Splash";
    }
}