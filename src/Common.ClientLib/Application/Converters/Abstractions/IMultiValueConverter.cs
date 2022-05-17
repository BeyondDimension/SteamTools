using System.Collections.Generic;
using System.Globalization;

// https://docs.microsoft.com/zh-cn/dotnet/api/system.windows.data.imultivalueconverter?view=windowsdesktop-6.0
// https://docs.microsoft.com/zh-cn/dotnet/api/xamarin.forms.imultivalueconverter.convertback?view=xamarin-forms

namespace System.Application.Converters.Abstractions;

/// <summary>
/// 提供在 MultiBinding 中应用自定义逻辑的方法。
/// </summary>
public interface IMultiValueConverter : IBinding
{
    /// <summary>
    /// 将源值转换为绑定目标的值。 数据绑定引擎在将该值从源绑定传播到绑定目标时会调用此方法。
    /// </summary>
    /// <param name="values">MultiBinding 中的源绑定生成的值的数组。 值 UnsetValue 指示源绑定没有可供转换的值。</param>
    /// <param name="targetType">绑定目标属性的类型。</param>
    /// <param name="parameter">要使用的转换器参数。</param>
    /// <param name="culture">要用在转换器中的区域性。</param>
    /// <returns>转换后的值。</returns>
    object? Convert(IList<object?> values, Type targetType, object? parameter, CultureInfo culture);

    /// <summary>
    /// 将绑定目标值转换为源绑定值。
    /// </summary>
    /// <param name="value">绑定目标生成的值。</param>
    /// <param name="targetTypes">要转换为的类型数组。 数组长度指示为要返回的方法所建议的值的数量与类型。</param>
    /// <param name="parameter">要使用的转换器参数。</param>
    /// <param name="culture">要用在转换器中的区域性。</param>
    /// <returns>已从目标值转换回源值的值的数组。</returns>
    object[]? ConvertBack(object? value, Type[] targetTypes, object? parameter, CultureInfo culture) => null;
}