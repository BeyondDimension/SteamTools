using Android.Content;
using AndroidX.Fragment.App;

namespace System.Application.UI
{
    public interface IViewHost
    {
        Context Context { get; }

        FragmentActivity Activity { get; }

        ViewHostType HostType { get; }
    }

    public enum ViewHostType : byte
    {
        Activity = 1,
        Fragment,
    }
}

namespace System.Application.UI.Activities
{
    partial class BaseActivity : IViewHost
    {
        Context IViewHost.Context => this;

        FragmentActivity IViewHost.Activity => this;

        ViewHostType IViewHost.HostType => ViewHostType.Activity;
    }

    partial class BaseActivity<TViewBinding, TViewModel> : IViewHost
    {
        Context IViewHost.Context => this;

        FragmentActivity IViewHost.Activity => this;

        ViewHostType IViewHost.HostType => ViewHostType.Activity;
    }
}

namespace System.Application.UI.Fragments
{
    partial class BaseFragment : IViewHost
    {
        Context IViewHost.Context => RequireContext();

        FragmentActivity IViewHost.Activity => RequireActivity();

        ViewHostType IViewHost.HostType => ViewHostType.Fragment;
    }

    partial class BaseFragment<TViewBinding, TViewModel> : IViewHost
    {
        Context IViewHost.Context => RequireContext();

        FragmentActivity IViewHost.Activity => RequireActivity();

        ViewHostType IViewHost.HostType => ViewHostType.Fragment;
    }
}