using System.Globalization;
using Avalonia.Data.Converters;

namespace System.Application.Converters
{
    public class SizeConverter : IValueConverter
    {
        private const int SizeUnit = 1024;

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (double.TryParse(value.ToString(), out double b))
            {
                if (b > 0)
                {
                    if (b > SizeUnit)
                    {
                        var kb = b / SizeUnit;
                        if (kb > SizeUnit)
                        {
                            var mb = kb / SizeUnit;
                            if (mb > SizeUnit)
                            {
                                var gb = mb / SizeUnit;
                                if (gb > SizeUnit)
                                {
                                    var tb = gb / SizeUnit;
                                    return tb.ToString("###,###.##") + " TB";
                                }
                                return gb.ToString("###,###.##") + " GB";
                            }
                            return mb.ToString("###,###.##") + " MB";
                        }
                        return kb.ToString("###,###.##") + " KB";
                    }
                    return b.ToString("###,###.##") + " B";
                }
                else 
                {
                    return "0 B";
                }
            }
            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}