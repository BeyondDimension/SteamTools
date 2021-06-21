using Android.Graphics;
using Android.Text;
using Android.Text.Method;
using Android.Views;
using Android.Widget;
using AndroidX.Annotations;
//using AndroidX.Emoji.Text;
using System;
using System.Common;
using System.Linq;
using Color = Android.Graphics.Color;

// ReSharper disable once CheckNamespace
namespace System
{
    /// <summary>
    /// 对 <see cref="TextView"/> 类的扩展函数集
    /// </summary>
    public static class TextViewExtensions
    {
        ///// <summary>
        ///// 设置 <see cref="TextView.Hint"/>，字符串中可能有 emoji 字符
        ///// <para>https://developer.android.google.cn/reference/android/widget/TextView#setHint(java.lang.CharSequence)</para>
        ///// </summary>
        ///// <param name="maybeEmojiStr">可能有 emoji 字符的字符串</param>
        //public static void SetHintByMaybeEmoji(this TextView textView, string maybeEmojiStr)
        //{
        //    // https://developer.android.google.cn/guide/topics/ui/look-and-feel/emoji-compat#using-emojicompat-without-widgets
        //    var processed = EmojiCompat.Get().ProcessFormatted(maybeEmojiStr.ToJavaString());
        //    textView.HintFormatted = processed;
        //}

        /// <summary>
        /// Sets the text color for all the states (normal, selected, focused) to be this color.
        /// <para>https://developer.android.google.cn/reference/android/widget/TextView#setTextColor(int)</para>
        /// </summary>
        /// <param name="textView"></param>
        /// <param name="color">A color value in the form 0xAARRGGBB. Do not pass a resource ID. To get a color value from a resource ID, call <see cref="ContextExtensions.GetColorCompat(Android.Content.Context, int)"/>.</param>
        public static void SetTextColor(this TextView textView, [ColorInt] int color)
        {
            var color_ = new Color(color);
            textView.SetTextColor(color_);
        }

        /// <summary>
        /// 添加或替换一组 <see cref="IInputFilter"/>
        /// </summary>
        /// <param name="textView"></param>
        /// <param name="filters"></param>
        public static void AddFilters(this TextView textView, params IInputFilter[] filters)
        {
            var items = textView.GetFilters();
            if (items != null && items.Any())
            {
                // 根据Type去重添加
                var types = filters.Where(x => x != null).Select(x => x.GetType()).Distinct();
                filters = items.Where(x => x != null && !types.Contains(x.GetType())).Concat(filters).ToArray();
            }
            textView.SetFilters(filters);
        }

        /// <summary>
        /// 给 <see cref="TextView.MovementMethod"/> 设置 <see cref="LinkMovementMethod.Instance"/>
        /// <para>功能用途：文字的超链接</para>
        /// </summary>
        /// <param name="textView"></param>
        public static void SetLinkMovementMethod(this TextView textView)
        {
            textView.MovementMethod = LinkMovementMethod.Instance;
        }

        /// <summary>
        /// 设置粗体
        /// </summary>
        /// <param name="textView"></param>
        /// <param name="isBold">是否粗体</param>
        /// <param name="isBoldOnly">是否全部文字使用粗体</param>
        /// <returns></returns>
        public static void SetBold(this TextView textView, bool isBold = true, bool isBoldOnly = true)
        {
            var paint = textView.Paint;
            if (paint != null)
            {
                paint.FakeBoldText = isBold;
                if (isBoldOnly)
                {
                    if (isBold)
                    {
                        textView.SetTypeface(textView.Typeface, TypefaceStyle.Bold);
                    }
                    else
                    {
                        textView.SetTypeface(textView.Typeface, TypefaceStyle.Normal);
                    }
                }
            }
        }

        /// <inheritdoc cref="SetText(TextView, string?, string, bool)"/>
        public static void SetText(this TextView textView, string? value, bool showVisibility = true)
        {
            var isNullOrEmpty = string.IsNullOrEmpty(value);
            textView.Text = value ?? string.Empty;
            if (!isNullOrEmpty)
            {
                if (showVisibility)
                {
                    if (textView.Visibility != ViewStates.Visible)
                    {
                        textView.Visibility = ViewStates.Visible;
                    }
                }
            }
        }

        /// <summary>
        /// 设置文本
        /// </summary>
        /// <param name="textView"></param>
        /// <param name="value">要设置的字符串</param>
        /// <param name="value2">如果要设置的字符串<see cref="string.IsNullOrEmpty(string)"/>则使用此值</param>
        /// <param name="showVisibility">如果为<see langword="true"/>，则当设置的字符串不为<see langword="null"/>或<see cref="string.Empty"/>时，使<see cref="View.Visibility"/>=<see cref="ViewStates.Visible"/></param>
        public static void SetText(this TextView textView, string? value, string value2, bool showVisibility = true)
        {
            value = string.IsNullOrEmpty(value) ? value2 : value;
            SetText(textView, value, showVisibility);
        }

        /// <summary>
        /// 设置仅能输入数字
        /// </summary>
        /// <param name="textView"></param>
        /// <param name="accepted"></param>
        public static void SetDigitsKeyListener(this TextView textView, string accepted = Constants.Digits)
        {
            textView.KeyListener = DigitsKeyListener.GetInstance(accepted);
        }
    }
}