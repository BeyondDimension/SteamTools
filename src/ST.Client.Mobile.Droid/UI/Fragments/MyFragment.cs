using Android.Runtime;
using Binding;

namespace System.Application.UI.Fragments
{
    [Register(JavaPackageConstants.Fragments + nameof(MyFragment))]
    internal sealed class MyFragment : BaseFragment<fragment_my>
    {
        protected override int? LayoutResource => Resource.Layout.fragment_my;
    }
}
