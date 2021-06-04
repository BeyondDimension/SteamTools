// https://github.com/xamarin/Xamarin.Forms/blob/5.0.0/Xamarin.Forms.Platform.iOS/Renderers/ButtonElementManager.cs
using System;
using UIKit;

// ReSharper disable once CheckNamespace
namespace Xamarin.Forms.Platform.iOS
{
    internal static class ButtonElementManager
    {
        static void TouchUpInside(object sender, EventArgs eventArgs)
        {
            if (sender is UIView button &&
                button.Superview is IVisualElementRenderer renderer &&
                renderer.Element is IButtonController element)
            {
                OnButtonTouchUpInside(element);
            }
        }

        static void TouchDown(object sender, EventArgs eventArgs)
        {
            if (sender is UIView button &&
               button.Superview is IVisualElementRenderer renderer &&
               renderer.Element is IButtonController element)
            {
                OnButtonTouchDown(element);
            }
        }

        internal static void OnButtonTouchDown(IButtonController element)
        {
            element?.SendPressed();
        }

        internal static void OnButtonTouchUpInside(IButtonController element)
        {
            element?.SendReleased();
            element?.SendClicked();
        }

        internal static void OnButtonTouchUpOutside(IButtonController element)
        {
            element?.SendReleased();
        }
    }
}