using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using SystemConvert = System.Convert;

namespace System.Application.Converters
{
    public class SwitchMultiValueConverter : IMultiValueConverter
    {
        public object? Convert(IList<object?> values, Type targetType, object? parameter, CultureInfo culture)
        {
            if (values.Any_Nullable())
            {
                var first = values.FirstOrDefault();
                if (first != null)
                {
                    try
                    {
                        var index = SystemConvert.ToInt32(first);
                        if (index > 0 && index < values.Count)
                        {
                            return values[index];
                        }
                    }
                    catch
                    {

                    }
                }
            }
            return ((IBinding)this).DoNothing;
        }
    }
}
