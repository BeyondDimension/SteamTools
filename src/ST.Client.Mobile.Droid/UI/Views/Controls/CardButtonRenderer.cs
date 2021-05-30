using Android.Content;
using System.Application.UI.Views.Controls;
using Xamarin.Forms;
using Xamarin.Forms.Material.Android;
using Xamarin.Forms.Platform.Android;
using AView = Android.Views.View;

[assembly: ExportRenderer(typeof(CardButton), typeof(CardButtonRenderer))]
namespace System.Application.UI.Views.Controls
{
    internal class CardButtonRenderer : MaterialFrameRenderer, AView.IOnClickListener
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