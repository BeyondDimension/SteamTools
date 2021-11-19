using System;
using System.Collections.Generic;
using System.Globalization;
using Avalonia.Data.Converters;

// ReSharper disable HeapView.BoxingAllocation

namespace System.Application.Converters
{
    public class RangeToSweepConverter : IMultiValueConverter
    {
        public object Convert(IList<object> values, Type targetType, object parameter, CultureInfo culture)
        {
            double min = 0, max = 100, val = 0;
            
            if (values[0] is double value)
            {
                val = value;
            }

            if (values.Count >= 2 && values[1] is double minimum)
            {
                min = minimum;
            }

            if (values.Count >= 3 && values[2] is double maximum)
            {
                max = maximum;
            }

            double m = max - min;
            var result = val / m;
            return result * 360;
        }
    }
}