using Android.Content;
using System.Application.UI.Fragments;
using System.Application.UI.Views;
using Xamarin.Forms;

[assembly: ExportRenderer(typeof(LocalAuthPage), typeof(LocalAuthPageRenderer))]
namespace System.Application.UI.Views
{
    internal sealed class LocalAuthPageRenderer : BaseFragmentPageRenderer<LocalAuthFragment>
    {
        public LocalAuthPageRenderer(Context context) : base(context)
        {
        }
    }
}
