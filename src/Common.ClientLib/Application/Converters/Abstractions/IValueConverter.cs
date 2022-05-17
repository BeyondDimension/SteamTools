using System.Globalization;

// https://docs.microsoft.com/zh-cn/dotnet/api/system.windows.data.ivalueconverter?view=windowsdesktop-6.0
// https://docs.microsoft.com/zh-cn/dotnet/api/Xamarin.Forms.IValueConverter?view=xamarin-forms
// https://docs.microsoft.com/zh-cn/uwp/api/Windows.UI.Xaml.Data.IValueConverter?view=winrt-22000

namespace System.Application.Converters.Abstractions;

/// <summary>
/// 提供将自定义逻辑应用于绑定的方法。
/// </summary>
public interface IValueConverter : IBinding
{
    /// <summary>
    /// 转换值。
    /// </summary>
    /// <param name="value">绑定源生成的值。</param>
    /// <param name="targetType">绑定目标属性的类型。</param>
    /// <param name="parameter">要使用的转换器参数。</param>
    /// <param name="culture">要用在转换器中的区域性。</param>
    /// <returns>转换后的值。 如果该方法返回 null，则使用有效的 null 值。</returns>
    object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture);

    /// <summary>
    /// 转换值。
    /// </summary>
    /// <param name="value">绑定目标生成的值。</param>
    /// <param name="targetType">要转换为的类型。</param>
    /// <param name="parameter">要使用的转换器参数。</param>
    /// <param name="culture">要用在转换器中的区域性。</param>
    /// <returns>转换后的值。 如果该方法返回 null，则使用有效的 null 值。</returns>
    object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture) => DoNothing;
}