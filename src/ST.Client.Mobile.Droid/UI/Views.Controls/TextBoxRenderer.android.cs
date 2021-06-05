using Android.Content;
using Android.Graphics.Drawables;
using Xamarin.Forms.Platform.Android;

namespace System.Application.UI.Views.Controls
{
    partial class TextBoxRenderer : EntryRenderer
    {
        public TextBoxRenderer(Context context)
            : base(context)
        {
        }

        protected override FormsEditText CreateNativeControl()
        {
            var editText = base.CreateNativeControl();
            editText.SetBackgroundResource(Resource.Drawable.bg_textbox);
            editText.TextSelectHandle = new ColorDrawable(Android.Graphics.Color.Transparent);
            editText.SetTextCursorDrawable(Resource.Drawable.text_cursor);
            var padding = Context!.DpToPxInt32(DefaultPaddingLR);
            editText.SetPadding(padding, editText.Top, padding, editText.Bottom);
            return editText;
        }
    }
}