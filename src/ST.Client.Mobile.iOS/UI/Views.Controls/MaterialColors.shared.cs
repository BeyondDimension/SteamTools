// https://github.com/xamarin/Xamarin.Forms/blob/5.0.0/Xamarin.Forms.Material.Android/MaterialColors.cs
// once we implement material configurations on core elements this can all be moved up to that
// for now just leaving this as an internal class to make matching colors between two platforms easier
#if __ANDROID__
using PlatformColor = Android.Graphics.Color;
#elif __IOS__
using MaterialComponents;
using PlatformColor = UIKit.UIColor;
#endif

#if __ANDROID__
// ReSharper disable once CheckNamespace
namespace Xamarin.Forms.Material.Android
#elif __IOS__
// ReSharper disable once CheckNamespace
namespace Xamarin.Forms.Material.iOS
#endif
{
    static partial class MaterialColors
    {
        static PlatformColor WithAlpha(PlatformColor color, float alpha)
        {
#if __ANDROID__
			return color.WithAlpha(alpha);
#elif __IOS__
            return color.ColorWithAlpha(alpha);
#endif
        }

        static PlatformColor FromRgb(int red, int green, int blue)
        {
#if __ANDROID__
			return PlatformColor.Rgb(red, green, blue);
#elif __IOS__
            return PlatformColor.FromRGB(red, green, blue);
#endif
        }

        public static partial class Light
        {
            // the Colors for "branding"
            //  - we selected the "black" theme from the default DarkActionBar theme
            public static readonly PlatformColor PrimaryColor = FromRgb(33, 33, 33);
            public static readonly PlatformColor PrimaryColorVariant = PlatformColor.Black;
            public static readonly PlatformColor OnPrimaryColor = PlatformColor.White;
            public static readonly PlatformColor SecondaryColor = FromRgb(33, 33, 33);
            public static readonly PlatformColor OnSecondaryColor = PlatformColor.White;
            public static readonly PlatformColor DisabledColor = WithAlpha(PlatformColor.Black, 0.38f);

            // the Colors for "UI"
            public static readonly PlatformColor BackgroundColor = PlatformColor.White;
            public static readonly PlatformColor OnBackgroundColor = PlatformColor.Black;
            public static readonly PlatformColor SurfaceColor = PlatformColor.White;
            public static readonly PlatformColor OnSurfaceColor = PlatformColor.Black;
            public static readonly PlatformColor ErrorColor = FromRgb(176, 0, 32);
            public static readonly PlatformColor OnErrorColor = PlatformColor.White;

#if __IOS__
            public static SemanticColorScheme CreateColorScheme()
            {
                return new SemanticColorScheme
                {
                    PrimaryColor = PrimaryColor,
                    PrimaryColorVariant = PrimaryColorVariant,
                    SecondaryColor = SecondaryColor,
                    OnPrimaryColor = OnPrimaryColor,
                    OnSecondaryColor = OnSecondaryColor,

                    BackgroundColor = BackgroundColor,
                    ErrorColor = ErrorColor,
                    SurfaceColor = SurfaceColor,
                    OnBackgroundColor = OnBackgroundColor,
                    OnSurfaceColor = OnSurfaceColor,
                };
            }
#endif
        }
    }
}