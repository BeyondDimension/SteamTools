using Android.Content;
using Android.Graphics.Drawables;
using System.Application.UI.Views.Controls;
using Xamarin.Forms;
using Xamarin.Forms.Platform.Android;

[assembly: ExportRenderer(typeof(TextBox), typeof(TextBoxRenderer))]
namespace System.Application.UI.Views.Controls
{
    internal class TextBoxRenderer : EntryRenderer
    {
        public TextBoxRenderer(Context context)
            : base(context)
        {
        }

        protected override FormsEditText CreateNativeControl()
        {
            var editText = base.CreateNativeControl();
            //editText.Background = null;
            editText.SetBackgroundResource(Resource.Drawable.bg_textbox);
            editText.TextSelectHandle = new ColorDrawable(Android.Graphics.Color.Transparent);
            editText.SetTextCursorDrawable(Resource.Drawable.text_cursor);
            var padding = Context.DpToPxInt32(10);
            editText.SetPadding(padding, editText.Top, padding, editText.Bottom);
            return editText;
        }
    }
}