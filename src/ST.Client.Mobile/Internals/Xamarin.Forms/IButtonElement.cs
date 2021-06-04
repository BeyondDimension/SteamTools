// https://github.com/xamarin/Xamarin.Forms/blob/5.0.0/Xamarin.Forms.Core/IButtonElement.cs
using System;
using System.ComponentModel;
using System.Windows.Input;

// ReSharper disable once CheckNamespace
namespace Xamarin.Forms.Internals
{
    [EditorBrowsable(EditorBrowsableState.Never)]
    internal interface IButtonElement
    {
        //note to implementor: implement this property publicly
        object CommandParameter { get; set; }
        ICommand Command { get; set; }
        bool IsPressed { get; }

        //note to implementor: but implement these methods explicitly
        void PropagateUpClicked();
        void PropagateUpPressed();
        void PropagateUpReleased();
        void SetIsPressed(bool isPressed);
        void OnCommandCanExecuteChanged(object sender, EventArgs e);
        bool IsEnabledCore { set; }
    }
}