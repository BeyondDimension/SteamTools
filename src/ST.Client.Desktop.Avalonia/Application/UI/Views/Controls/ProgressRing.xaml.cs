using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;

namespace System.Application.UI.Views.Controls
{
    public class ProgressRing : TemplatedControl
    {
        public static readonly StyledProperty<bool> IsIndeterminateProperty =
            AvaloniaProperty.Register<ProgressRing, bool>(nameof(IsIndeterminate));

        public static readonly StyledProperty<double> PercentageProperty =
            AvaloniaProperty.Register<ProgressRing, double>(nameof(Percentage));

        public static readonly StyledProperty<double> StrokeThicknessProperty =
            AvaloniaProperty.Register<ProgressRing, double>(nameof(StrokeThickness));

        public bool IsIndeterminate
        {
            get => GetValue(IsIndeterminateProperty);
            set => SetValue(IsIndeterminateProperty, value);
        }

        public double Percentage
        {
            get => GetValue(PercentageProperty);
            set => SetValue(PercentageProperty, value);
        }

        public double StrokeThickness
        {
            get => GetValue(StrokeThicknessProperty);
            set => SetValue(StrokeThicknessProperty, value);
        }

        protected override void OnPropertyChanged<T>(AvaloniaPropertyChangedEventArgs<T> e)
        {
            base.OnPropertyChanged(e);

            if (e.Property == IsIndeterminateProperty)
            {
                PseudoClasses.Set(":indeterminate", IsIndeterminate);
            }
        }
    }
}
