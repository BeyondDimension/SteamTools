#if AVALONIA
using _Binding = Avalonia.Data.BindingOperations;
using _DependencyProperty = Avalonia.AvaloniaProperty;
#elif MAUI
using _Binding = Microsoft.Maui.Controls.Binding;
using _DependencyProperty = Microsoft.Maui.Controls.BindableProperty;
#elif __MOBILE__
using _Binding = Xamarin.Forms.Binding;
using _DependencyProperty = Xamarin.Forms.BindableProperty;
#endif
using BaseType = System.Application.Converters.Abstractions.IBinding;

namespace System.Application.Converters;

/// <inheritdoc cref="BaseType"/>
public interface IBinding : BaseType
{
    object BaseType.DoNothing => _Binding.DoNothing;

    object BaseType.UnsetValue => _DependencyProperty.UnsetValue;
}