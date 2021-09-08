using UIKit;
using Xamarin.Forms.Material.iOS;

namespace System.Application.UI.Views.Controls
{
    partial class CardButtonRenderer : MaterialFrameRenderer
    {
        public CardButtonRenderer() : base()
        {
            //UserInteractionEnabled = true;
            //EnableRippleBehavior = true;

            var tapGesture = new UITapGestureRecognizer(() => Element.Command.Invoke(Element.CommandParameter));
            AddGestureRecognizer(tapGesture);
        }

        public new CardButton Element => (CardButton)base.Element;
    }
}