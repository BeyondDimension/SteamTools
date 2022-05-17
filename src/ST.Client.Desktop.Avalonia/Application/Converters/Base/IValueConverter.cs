using System.Globalization;
#if AVALONIA
using _IValueConverter = Avalonia.Data.Converters.IValueConverter;
#elif MAUI
using _IValueConverter = Microsoft.Maui.Controls.IValueConverter;
#elif __MOBILE__
using _IValueConverter = Xamarin.Forms.IValueConverter;
#endif
using BaseType = System.Application.Converters.Abstractions.IValueConverter;

namespace System.Application.Converters;

/// <inheritdoc cref="BaseType"/>
public interface IValueConverter : BaseType, _IValueConverter, IBinding
{
    object? _IValueConverter.ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        BaseType thiz = this;
        return thiz.ConvertBack(value, targetType, parameter, culture);
    }
}