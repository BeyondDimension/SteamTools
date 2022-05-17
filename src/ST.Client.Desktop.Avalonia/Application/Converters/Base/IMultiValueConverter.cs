#if AVALONIA
using _IMultiValueConverter = Avalonia.Data.Converters.IMultiValueConverter;
#elif MAUI
using System.Globalization;
using System.Collections.Generic;
using _IMultiValueConverter = Microsoft.Maui.Controls.IMultiValueConverter;
#elif __MOBILE__
using System.Globalization;
using System.Collections.Generic;
using _IMultiValueConverter = Xamarin.Forms.IMultiValueConverter;
#endif
using BaseType = System.Application.Converters.Abstractions.IMultiValueConverter;

namespace System.Application.Converters;

/// <inheritdoc cref="BaseType"/>
public partial interface IMultiValueConverter : BaseType, _IMultiValueConverter, IBinding
{
#if __MOBILE__ || MAUI
    object? _IMultiValueConverter.Convert(object?[] values, Type targetType, object? parameter, CultureInfo culture)
    {
        IList<object?>? values_ = values;
        return Convert(values_, targetType, parameter, culture);
    }
#endif

#if __MOBILE__ || MAUI
    object[]? _IMultiValueConverter.ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
    {
        BaseType thiz = this;
        return thiz.ConvertBack(value, targetTypes, parameter, culture);
    }
#endif
}