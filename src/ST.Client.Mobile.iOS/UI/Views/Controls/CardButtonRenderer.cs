using System.Application.UI.Views.Controls;
using UIKit;
using Xamarin.Forms;
using Xamarin.Forms.Material.iOS;
using Xamarin.Forms.Platform.iOS;

[assembly: ExportRenderer(typeof(CardButton), typeof(CardButtonRenderer))]
namespace System.Application.UI.Views.Controls
{
    internal class CardButtonRenderer : MaterialFrameRenderer
    {
        public CardButtonRenderer() : base()
        {
            EnableRippleBehavior = true;

            this.TouchUpInside += OnButtonTouchUpInside;
            this.TouchUpOutside += OnButtonTouchUpOutside;
            this.TouchDown += OnButtonTouchDown;
        }

        public new CardButton Element
        {
            get => (CardButton)base.Element;
        }

        public UIView Control => this;

        void OnButtonTouchUpInside(object sender, EventArgs eventArgs)
        {
            ButtonElementManager.OnButtonTouchUpInside(this.Element);
        }

        void OnButtonTouchUpOutside(object sender, EventArgs eventArgs)
        {
            ButtonElementManager.OnButtonTouchUpOutside(this.Element);
        }

        void OnButtonTouchDown(object sender, EventArgs eventArgs)
        {
            ButtonElementManager.OnButtonTouchDown(this.Element);
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);

            if (disposing)
            {
                this.TouchUpInside -= OnButtonTouchUpInside;
                this.TouchUpOutside -= OnButtonTouchUpOutside;
                this.TouchDown -= OnButtonTouchDown;
            }
        }
    }
}