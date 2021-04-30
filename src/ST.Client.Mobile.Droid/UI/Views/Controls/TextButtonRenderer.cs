using Android.Content;
using Android.Runtime;
using Android.Views;
using AndroidX.AppCompat.Widget;
using System.Application.UI.Views.Controls;
using Xamarin.Forms;

[assembly: ExportRenderer(typeof(TextButton), typeof(TextButtonRenderer))]
namespace System.Application.UI.Views.Controls
{
    internal class TextButtonRenderer : Xamarin.Forms.Platform.Android.AppCompat.ButtonRenderer
    {
        public TextButtonRenderer(Context context)
            : base(context)
        {
        }

        protected override AppCompatButton CreateNativeControl()
        {
            var view = LayoutInflater.From(Context).Inflate(Resource.Layout.controls_btntext, null).JavaCast<AppCompatButton>();
            //view.Background = null;
            return view;
        }
    }
}