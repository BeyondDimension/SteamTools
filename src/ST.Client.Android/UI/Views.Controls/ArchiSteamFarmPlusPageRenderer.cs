using Android.Content;
using System.Application.UI.Fragments;
using System.Application.UI.Views;
using Xamarin.Forms;

[assembly: ExportRenderer(typeof(ArchiSteamFarmPlusPage), typeof(ArchiSteamFarmPlusPageRenderer))]
namespace System.Application.UI.Views
{
    internal sealed class ArchiSteamFarmPlusPageRenderer : BaseFragmentPageRenderer<ASFPlusFragment>
    {
        public ArchiSteamFarmPlusPageRenderer(Context context) : base(context)
        {

        }
    }
}
