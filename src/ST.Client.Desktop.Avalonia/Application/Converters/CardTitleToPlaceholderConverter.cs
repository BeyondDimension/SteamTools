using System.Globalization;
using System.Linq;
using Avalonia;
using Avalonia.Data.Converters;
using Avalonia.Media;

namespace System.Application.Converters
{
    public class CardTitleToPlaceholderConverter : IValueConverter
    {
        private static readonly (Color StartColor, Color EndColor)[] placeholderColors =
        {
            (Color.Parse("#38ADAE"), Color.Parse("#CD295A")),
            (Color.Parse("#F1EAB9"), Color.Parse("#FF8C8C")),
            (Color.Parse("#C6EA8D"), Color.Parse("#FE90AF")),
            (Color.Parse("#EA8D8D"), Color.Parse("#A890FE")),
            (Color.Parse("#D8B5FF"), Color.Parse("#1EAE98")),
            (Color.Parse("#FF61D2"), Color.Parse("#FE9090")),
            (Color.Parse("#BFF098"), Color.Parse("#6FD6FF")),
            (Color.Parse("#4E65FF"), Color.Parse("#92EFFD")),
            (Color.Parse("#A9F1DF"), Color.Parse("#FFBBBB"))
        };

        public object? Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string s)
            {
                var selector = s.Select(x => (uint) x)
                    .Aggregate((x, y) => x ^ y) % (uint) placeholderColors.Length;

                return new LinearGradientBrush
                {
                    StartPoint = new RelativePoint(0, 0, RelativeUnit.Relative),
                    EndPoint = new RelativePoint(1, 1, RelativeUnit.Relative),
                    GradientStops = new GradientStops
                    {
                        new GradientStop(placeholderColors[selector].StartColor, 0),
                        new GradientStop(placeholderColors[selector].EndColor, 1)
                    }
                };
            }

            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}