using Android.Views;
using System;
using Color = Android.Graphics.Color;

// ReSharper disable once CheckNamespace
namespace Android.Text.Style
{
    /// <summary>
    /// 富文本中超链接的 Span，类似于 WPF 中的 Hyperlink/Span/Inline/Run
    /// </summary>
    public sealed class HyperlinkClickableSpan : ClickableSpan
    {
        readonly Action<View?> onClick;
        readonly bool underlineText;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="onClick">点击事件委托</param>
        /// <param name="underlineText">是否需要下划线，默认值 <see langword="false"/></param>
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