using Android.Content;
using Android.Runtime;
using Android.Views;
using AndroidX.AppCompat.Widget;
using ButtonRenderer = Xamarin.Forms.Platform.Android.AppCompat.ButtonRenderer;

namespace System.Application.UI.Renderers
{
    partial class TextButtonRenderer : ButtonRenderer
    {
        public TextButtonRenderer(Context context)
            : base(context)
        {
        }

        protected override AppCompatButton CreateNativeControl()
        {
            var view = LayoutInflater.From(Context)!.Inflate(Resource.Layout.controls_btntext, null).JavaCast<AppCompatButton>()!;
            //view.Background = null;
            return view;
        }
    }
}