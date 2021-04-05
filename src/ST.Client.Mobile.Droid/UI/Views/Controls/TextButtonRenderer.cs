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
#pragma warning disable CS8603 // 可能的 null 引用返回。
#pragma warning disable CS8602 // 解引用可能出现空引用。
            return LayoutInflater.From(Context).Inflate(Resource.Layout.controls_btntext, null).JavaCast<AppCompatButton>();
#pragma warning restore CS8602 // 解引用可能出现空引用。
#pragma warning restore CS8603 // 可能的 null 引用返回。
        }
    }
}