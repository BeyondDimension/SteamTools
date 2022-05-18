using Android.App;
//using Android.Views;
using Android.Runtime;
using Xamarin.Android.Design;
using Google.Android.Material.AppBar;
using R = System.Application.Resource;

#pragma warning disable IDE1006 // 命名样式
namespace Binding
{
    sealed class activity_material_toolbar_base : LayoutBinding
    {
        [Preserve(Conditional = true)]
        public activity_material_toolbar_base(
            Activity client,
            OnLayoutItemNotFoundHandler? itemNotFoundHandler = null)
                : base(client, itemNotFoundHandler!)
        { }

        //[Preserve(Conditional = true)]
        //public activity_material_toolbar_base(
        //    View client,
        //    OnLayoutItemNotFoundHandler? itemNotFoundHandler = null)
        //        : base(client, itemNotFoundHandler!)
        //{ }

        MaterialToolbar? __toolbar;

        public MaterialToolbar toolbar => FindView(R.Id.toolbar, ref __toolbar);
    }
}

#pragma warning restore IDE1006 // 命名样式