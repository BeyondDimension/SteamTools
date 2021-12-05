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
            ConfigChanges.Locale |
            ConfigChanges.LayoutDirection |
            ConfigChanges.SmallestScreenSize;

        public const string MainTheme2 = "@style/MainTheme2";
        public const string MainTheme2_NoActionBar = "@style/MainTheme2.NoActionBar";
        public const string MainTheme2_Splash = "@style/MainTheme2.Splash";
    }
}