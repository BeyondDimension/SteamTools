using Android.Content;
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
    }
}