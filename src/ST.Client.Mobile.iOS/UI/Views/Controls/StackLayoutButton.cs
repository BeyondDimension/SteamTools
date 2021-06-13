using System.ComponentModel;
using System.Windows.Input;
using Xamarin.Forms;
using Xamarin.Forms.Internals;

namespace System.Application.UI.Views.Controls
{
    public class StackLayoutButton : StackLayout, IButtonController, IButtonElement
    {
        public static readonly BindableProperty CommandProperty = ButtonElement.CommandProperty;

        public static readonly BindableProperty CommandParameterProperty = ButtonElement.CommandParameterProperty;

        public ICommand Command
        {
            get { return (ICommand)GetValue(CommandProperty); }
            set { SetValue(CommandProperty, value); }
        }
        public object CommandParameter
        {
            get { return GetValue(CommandParameterProperty); }
            set { SetValue(CommandParameterProperty, value); }
        }

        internal static readonly BindablePropertyKey IsPressedPropertyKey = BindableProperty.CreateReadOnly(nameof(IsPressed), typeof(bool), typeof(Button), default(bool));
        public static readonly BindableProperty IsPressedProperty = IsPressedPropertyKey.BindableProperty;

        public bool IsPressed => (bool)GetValue(IsPressedProperty);

        public event EventHandler? Clicked;
        public event EventHandler? Pressed;

        public event EventHandler? Released;

        [EditorBrowsable(EditorBrowsableState.Never)]
        void IButtonElement.SetIsPressed(bool isPressed) => SetValue(IsPressedPropertyKey, isPressed);

        [EditorBrowsable(EditorBrowsableState.Never)]
        public void SendClicked() => ButtonElement.ElementClicked(this, this);

        [EditorBrowsable(EditorBrowsableState.Never)]
        public void SendPressed() => ButtonElement.ElementPressed(this, this);

        [EditorBrowsable(EditorBrowsableState.Never)]
        public void SendReleased() => ButtonElement.ElementReleased(this, this);

        [EditorBrowsable(EditorBrowsableState.Never)]
        void IButtonElement.PropagateUpClicked() => Clicked?.Invoke(this, EventArgs.Empty);

        [EditorBrowsable(EditorBrowsableState.Never)]
        void IButtonElement.PropagateUpPressed() => Pressed?.Invoke(this, EventArgs.Empty);

        [EditorBrowsable(EditorBrowsableState.Never)]
        void IButtonElement.PropagateUpReleased() => Released?.Invoke(this, EventArgs.Empty);

        void IButtonElement.OnCommandCanExecuteChanged(object sender, EventArgs e) =>
            ButtonElement.CommandCanExecuteChanged(this, EventArgs.Empty);

        bool IButtonElement.IsEnabledCore
        {
            set { SetValueCore(IsEnabledProperty, value); }
        }
    }
}