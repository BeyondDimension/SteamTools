using Android.OS;
using Android.Views;
using ReactiveUI.AndroidX;
using System.Application.Mvvm;
using System.Application.UI.ViewModels;
using System.Collections.Generic;
using System.Reactive.Disposables;
using Fragment = AndroidX.Fragment.App.Fragment;

// ReSharper disable once CheckNamespace
namespace System.Application.UI.Fragments
{
    /// <summary>
    /// 当前应用的 <see cref="Fragment"/> 基类
    /// <list type="bullet">
    ///     <item>Fragment(<see href="https://developer.android.google.cn/guide/fragments"/>)</item>
    ///     <item>Fragment 表示应用界面中可重复使用的一部分。</item>
    ///     <item>Fragment 定义和管理自己的布局，具有自己的生命周期，并且可以处理自己的输入事件。</item>
    ///     <item>Fragment 不能独立存在，而是必须由 Activity 或另一个 Fragment 托管。</item>
    ///     <item>Fragment 的视图层次结构会成为宿主的视图层次结构的一部分，或附加到宿主的视图层次结构。</item>
    /// </list>
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
            base.OnDestroyView();
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

    /// <summary>
    /// 当前应用的 <see cref="ReactiveFragment"/>(ReactiveUI) 基类
    /// <list type="bullet">
    ///     <item>ReactiveFragment(<see href="https://github.com/reactiveui/ReactiveUI/blob/main/src/ReactiveUI.AndroidX/ReactiveFragment.cs"/>)</item>
    ///     <item>Fragment(<see href="https://developer.android.google.cn/guide/fragments"/>)</item>
    ///     <item>Fragment 表示应用界面中可重复使用的一部分。</item>
    ///     <item>Fragment 定义和管理自己的布局，具有自己的生命周期，并且可以处理自己的输入事件。</item>
    ///     <item>Fragment 不能独立存在，而是必须由 Activity 或另一个 Fragment 托管。</item>
    ///     <item>Fragment 的视图层次结构会成为宿主的视图层次结构的一部分，或附加到宿主的视图层次结构。</item>
    /// </list>
    /// </summary>
    /// <typeparam name="TViewBinding"></typeparam>
    /// <typeparam name="TViewModel"></typeparam>
    public abstract partial class BaseFragment<TViewBinding, TViewModel> : ReactiveFragment<TViewModel>, ReactiveUI.IReadOnlyViewFor<TViewModel>, IDisposableHolder
        where TViewBinding : class
        where TViewModel : ViewModelBase
    {
        readonly CompositeDisposable disposables = new();
        ICollection<IDisposable> IDisposableHolder.CompositeDisposable => disposables;

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            var view = BaseFragment.CreateView(LayoutResource, inflater, container);
            if (view == null)
                return base.OnCreateView(inflater, container, savedInstanceState);
            ViewModel = this.GetViewModel<TViewModel>();
            OnCreateView(view);
            return view;
        }

        public override void OnDestroyView()
        {
            ClearOnClickListener();
            base.OnDestroyView();
            disposables.Dispose();
            binding = null;
        }
    }
}

// ReSharper disable once CheckNamespace
namespace System
{
    public static partial class BaseUIExtensions
    {
        public static TViewModel GetViewModel<TViewModel>(this Fragment fragment)
            where TViewModel : class, IDisposable
        {
            if (fragment.Activity is ReactiveUI.IReadOnlyViewFor<TViewModel> rvf && rvf.ViewModel != null)
            {
                return rvf.ViewModel;
            }

            if (fragment.Activity is ReactiveUI.IViewFor<TViewModel> vf && vf.ViewModel != null)
            {
                return vf.ViewModel;
            }

            if (fragment is IDisposableHolder dh)
            {
                TViewModel r = Activator.CreateInstance<TViewModel>();
                r.AddTo(dh);
                return r;
            }
            else
            {
                throw new InvalidOperationException("fragment must implement IDisposableHolder.");
            }
        }
    }
}