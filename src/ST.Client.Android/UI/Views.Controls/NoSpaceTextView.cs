using Android.Content;
using Android.Graphics;
using Android.Runtime;
using Android.Util;
using AndroidX.AppCompat.Widget;
using CharSequence = Java.Lang.ICharSequence;

namespace System.Application.UI.Views.Controls
{
    /// <summary>
    /// https://github.com/wwluo14/NoSpaceTextView/blob/master/app/src/main/java/com/demo/nospacetextview/NoSpaceTextView.java
    /// </summary>
    [Register("com.github.wwluo14.nospacetextview.NoSpaceTextView")]
    public class NoSpaceTextView : AppCompatTextView
    {
        bool refreshMeasure = false;

        protected NoSpaceTextView(IntPtr javaReference, JniHandleOwnership transfer) : base(javaReference, transfer)
        {

        }

        public NoSpaceTextView(Context context) : base(context)
        {
        }

        public NoSpaceTextView(Context context, IAttributeSet attrs) : base(context, attrs)
        {
        }

        public NoSpaceTextView(Context context, IAttributeSet attrs, int defStyleAttr) : base(context, attrs, defStyleAttr)
        {
        }

        protected override void OnMeasure(int widthMeasureSpec, int heightMeasureSpec)
        {
            base.OnMeasure(widthMeasureSpec, heightMeasureSpec);
            RemoveSpace(widthMeasureSpec, heightMeasureSpec);
        }

        public override void SetText(CharSequence? text, BufferType? type)
        {
            base.SetText(text, type);
            // 每次文本内容改变时，需要测量两次，确保计算的高度没有问题
            refreshMeasure = true;
        }

        /// <summary>
        /// 处理文本的上下留白问题
        /// </summary>
        /// <param name="widthspc"></param>
        /// <param name="heightspc"></param>
        void RemoveSpace(int widthspc, int heightspc)
        {
            var linesText = LinesText;
            var paint = Paint!;
            Rect rect = new();
            var text = linesText[0];
            paint.GetTextBounds(text, 0, text.Length, rect);

            Paint.FontMetricsInt fontMetricsInt = new();
            paint.GetFontMetricsInt(fontMetricsInt);

            var paddingTop = fontMetricsInt.Top - rect.Top;

            // 设置TextView向上的padding (小于0, 即把TextView文本内容向上移动)
            SetPadding(LeftPaddingOffset
                    , paddingTop + TopPaddingOffset
                    , RightPaddingOffset
                    , BottomPaddingOffset);

            var endText = linesText[^1];
            paint.GetTextBounds(endText, 0, endText.Length, rect);

            // 再减去最后一行文本的底部空白，得到的就是TextView内容上线贴边的的高度，到达消除文本上下留白的问题
            SetMeasuredDimension(MeasuredWidth
                    , MeasuredHeight - (fontMetricsInt.Bottom - rect.Bottom));

            if (refreshMeasure)
            {
                refreshMeasure = false;
                Measure(widthspc, heightspc);
            }
        }

        /// <summary>
        /// 每一行的文本内容
        /// </summary>
        /// <returns></returns>
        string[] LinesText
        {
            get
            {
                int start = 0;
                var lineCount = LineCount;
                var texts = new string[lineCount];
                var text = Text;
                var layout = Layout;

                for (int i = 0; i < lineCount; i++)
                {
                    var end = layout!.GetLineEnd(i);
                    var line = text!.JavaSubstring(start, end); //指定行的内容
                    start = end;

                    texts[i] = line;
                }

                return texts;
            }
        }
    }
}