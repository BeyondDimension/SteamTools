// https://github.com/xamarin/Xamarin.Forms/blob/5.0.0/Xamarin.Forms.Core/ButtonElement.cs
using System;
using System.Windows.Input;
using Xamarin.Forms.Internals;

// ReSharper disable once CheckNamespace
namespace Xamarin.Forms
{
    internal static class ButtonElement
    {
        public static readonly BindableProperty CommandProperty = BindableProperty.Create(nameof(IButtonElement.Command), typeof(ICommand), typeof(IButtonElement), null, propertyChanging: OnCommandChanging, propertyChanged: OnCommandChanged);

        public static readonly BindableProperty CommandParameterProperty = BindableProperty.Create(nameof(IButtonElement.CommandParameter), typeof(object), typeof(IButtonElement), null,
            propertyChanged: (bindable, oldvalue, newvalue) => CommandCanExecuteChanged(bindable, EventArgs.Empty));

        static void OnCommandChanged(BindableObject bo, object o, object n)
        {
            IButtonElement button = (IButtonElement)bo;
            if (n is ICommand newCommand)
                newCommand.CanExecuteChanged += button.OnCommandCanExecuteChanged;

            CommandChanged(button);
        }

        static void OnCommandChanging(BindableObject bo, object o, object n)
        {
            IButtonElement button = (IButtonElement)bo;
            if (o != null)
            {
                (o as ICommand).CanExecuteChanged -= button.OnCommandCanExecuteChanged;
            }
        }

        public const string PressedVisualState = "Pressed";

        public static void CommandChanged(IButtonElement sender)
        {
            if (sender.Command != null)
            {
                CommandCanExecuteChanged(sender, EventArgs.Empty);
            }
            else
            {
                sender.IsEnabledCore = true;
            }
        }

        public static void CommandCanExecuteChanged(object sender, EventArgs e)
        {
            IButtonElement ButtonElementManager = (IButtonElement)sender;
            ICommand cmd = ButtonElementManager.Command;
            if (cmd != null)
            {
                ButtonElementManager.IsEnabledCore = cmd.CanExecute(ButtonElementManager.CommandParameter);
            }
        }

        public static void ElementClicked(VisualElement visualElement, IButtonElement ButtonElementManager)
        {
            if (visualElement.IsEnabled)
            {
                ButtonElementManager.Command?.Execute(ButtonElementManager.CommandParameter);
                ButtonElementManager.PropagateUpClicked();
            }
        }

        public static void ElementPressed(VisualElement visualElement, IButtonElement ButtonElementManager)
        {
            if (visualElement.IsEnabled)
            {
                ButtonElementManager.SetIsPressed(true);
                visualElement.ChangeVisualStateInternal();
                ButtonElementManager.PropagateUpPressed();
            }
        }

        public static void ElementReleased(VisualElement visualElement, IButtonElement ButtonElementManager)
        {
            if (visualElement.IsEnabled)
            {
                IButtonController _ = ButtonElementManager as IButtonController;
                ButtonElementManager.SetIsPressed(false);
                visualElement.ChangeVisualStateInternal();
                ButtonElementManager.PropagateUpReleased();
            }
        }
    }
}