using Android.Content;
using Xamarin.Forms.Material.Android;
using Xamarin.Forms.Platform.Android;
using AView = Android.Views.View;

namespace System.Application.UI.Views.Controls
{
    partial class CardButtonRenderer : MaterialFrameRenderer, AView.IOnClickListener
    {
        public CardButtonRenderer(Context context)
          : base(context)
        {
            Clickable = true;
            SetOnClickListener(this);
        }

        protected new CardButton Element
        {
            get => (CardButton)base.Element;
            set => base.Element = value;
        }

        void IOnClickListener.OnClick(AView? v) => ButtonElementManager.OnClick(Element, Element, v);
    }
}