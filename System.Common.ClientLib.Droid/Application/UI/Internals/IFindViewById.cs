using Android.Views;
using System.Application.UI.Internals;

namespace System.Application.UI.Internals
{
    /// <summary>
    /// UI - 根据视图Id查找
    /// </summary>
    internal interface IFindViewById
    {
        T? FindViewById<T>(int id) where T : View;

        View? FindViewById(int id);
    }
}

namespace System.Application.UI.Activities
{
    partial class BaseActivity : IFindViewById
    {
    }
}

namespace System.Application.UI.Fragments
{
    partial class BaseFragment : IFindViewById
    {
        View? mView;

        public new View View
        {
            get
            {
                return mView ?? base.View;
            }
            set
            {
                mView = value;
            }
        }

        T? IFindViewById.FindViewById<T>(int id) where T : class => IFragment.FindViewById<T>(this, id);

        View? IFindViewById.FindViewById(int id) => IFragment.FindViewById(this, id);
    }

    partial class BaseDialogFragment : IFindViewById
    {
        View? mView;

        public new View View
        {
            get
            {
                return mView ?? base.View;
            }
            set
            {
                mView = value;
            }
        }

        T? IFindViewById.FindViewById<T>(int id) where T : class => IFragment.FindViewById<T>(this, id);

        View? IFindViewById.FindViewById(int id) => IFragment.FindViewById(this, id);
    }

    partial class BaseBottomSheetDialogFragment : IFindViewById
    {
        View? mView;

        public new View View
        {
            get
            {
                return mView ?? base.View;
            }
            set
            {
                mView = value;
            }
        }

        T? IFindViewById.FindViewById<T>(int id) where T : class => IFragment.FindViewById<T>(this, id);

        View? IFindViewById.FindViewById(int id) => IFragment.FindViewById(this, id);
    }
}