using Android.Views;
using System;
using Color = Android.Graphics.Color;

// ReSharper disable once CheckNamespace
namespace Android.Text.Style
{
    public sealed class HyperlinkClickableSpan : ClickableSpan
    {
        readonly Action<View?> onClick;
        readonly bool underlineText;

        public HyperlinkClickableSpan(Action<View?> onClick, bool underlineText = false)
        {
            this.onClick = onClick;
            this.underlineText = underlineText;
        }

        public override void OnClick(View widget) => onClick(widget);

        public override void UpdateDrawState(TextPaint ds)
        {
            ds.Color = new Color(ds.LinkColor);
            ds.UnderlineText = underlineText;
        }
    }
}