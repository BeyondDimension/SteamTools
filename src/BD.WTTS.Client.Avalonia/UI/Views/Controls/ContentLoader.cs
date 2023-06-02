using Avalonia.Controls;

namespace BD.WTTS.UI.Views.Controls;

public class ContentLoader : ContentControl
{
    /// <summary>
    /// Defines the <see cref="IsLoading"/> property
    /// </summary>
    public static readonly StyledProperty<bool> IsLoadingProperty =
        AvaloniaProperty.Register<ContentLoader, bool>(nameof(IsLoading), true);

    /// <summary>
    /// Defines the <see cref="IsLoading"/> property
    /// </summary>
    public static readonly StyledProperty<string?> NoResultMessageProperty =
        AvaloniaProperty.Register<ContentLoader, string?>(nameof(NoResultMessage), null);

    /// <summary>
    /// 是否正在加载中
    /// </summary>
    public bool IsLoading
    {
        get => GetValue(IsLoadingProperty);
        set => SetValue(IsLoadingProperty, value);
    }

    /// <summary>
    /// 无结果时提示
    /// </summary>
    public string? NoResultMessage
    {
        get => GetValue(NoResultMessageProperty);
        set => SetValue(NoResultMessageProperty, value);
    }
}
