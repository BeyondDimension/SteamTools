using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BD.WTTS.Converters;

public sealed class StringOmitConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
    {
        string s = value.ToString();
        int leng;
        if (int.TryParse(parameter.ToString(), out leng))
        {
            if (s.Length <= leng)
                return s;
            else
                return s[..leng] + "...";
        }
        else
            return string.Empty;
    }

    public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
