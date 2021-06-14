using Android.OS;
using Android.Views;
using ReactiveUI.AndroidX;
using System.Application.UI.ViewModels;
using Fragment = AndroidX.Fragment.App.Fragment;

// ReSharper disable once CheckNamespace
namespace System.Application.UI.Fragments
{
    /// <summary>
    /// 当前应用的 <see cref="Fragment"/> 基类
    /// </summary>
    public abstract partial class BaseFragment : Fragment
    {
        public virtual void OnCreateView(View view)
        {
        }

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            var view = CreateView(LayoutResource, inflater, container);
            if (view == null)
                return base.OnCreateView(inflater, container, savedInstanceState);
            OnCreateView(view);
            return view;
        }

        public override void OnDestroyView()
        {
            ClearOnClickListener();
            base.OnDestroy();
        }
    }

    /// <inheritdoc cref="BaseFragment"/>
    public abstract partial class BaseFragment<TViewBinding> : BaseFragment where TViewBinding : class
    {
        public override void OnDestroyView()
        {
            base.OnDestroyView();
            binding = null;
        }
    }

    /// <inheritdoc cref="BaseFragment"/>
    public abstract partial class BaseFragment<TViewBinding, TViewModel> : ReactiveFragment<TViewModel>
        where TViewBinding : class
        where TViewModel : ViewModelBase
    {
        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            var view = BaseFragment.CreateView(LayoutResource, inflater, container);
            if (view == null)
                return base.OnCreateView(inflater, container, savedInstanceState);
            OnCreateView(view);
            return view;
        }

        public override void OnDestroyView()
        {
            ClearOnClickListener();
            base.OnDestroy();
            binding = null;
        }
    }
}