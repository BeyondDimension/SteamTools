using System.Application.UI.Views.Controls;
using Xamarin.Forms;
using XFRecyclerView = System.Application.UI.Views.Controls.RecyclerView;

[assembly: ExportRenderer(typeof(XFRecyclerView), typeof(RecyclerViewRenderer))]

namespace System.Application.UI.Views.Controls
{
    internal partial class RecyclerViewRenderer
    {
    }
}
