namespace System.Application.Converters.Abstractions;

/// <summary>
/// 提供对绑定定义的高级访问，该绑定连接绑定目标对象（通常为 WPF 元素）的属性和任何数据源（例如数据库、XML 文件，或包含数据的任何对象）。
/// </summary>
public interface IBinding
{
    /// <summary>
    /// 用作返回值以指示绑定引擎不执行任何操作。
    /// </summary>
    object DoNothing { get; }

    /// <summary>
    /// 指定 WPF 属性系统使用的静态值，而不是 null 指示属性存在，但不具有由属性系统设置的值。
    /// </summary>
    object UnsetValue { get; }
}