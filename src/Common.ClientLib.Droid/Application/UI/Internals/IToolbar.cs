using AndroidX.AppCompat.App;
using AndroidX.AppCompat.Widget;
using System.Application.UI.Internals;
using System.Common;
using Fragment = AndroidX.Fragment.App.Fragment;

namespace System.Application.UI.Internals
{
    /// <summary>
    /// UI - 工具栏
    /// </summary>
    internal interface IToolbar : IFindViewById
    {
        /// <summary>
        /// 当前页面是否有 <see cref="Toolbar"/>
        /// </summary>
        bool HasToolbar { get; }

        /// <summary>
        /// 设置默认 <see cref="ActionBar.Title"/> (与 <see cref="DefaultActionBarTitleResource"/> 参数互斥)
        /// </summary>
        string? DefaultActionBarTitleString { get; }

        /// <summary>
        /// 设置默认 <see cref="ActionBar.Title"/> (与 <see cref="DefaultActionBarTitleString"/> 参数互斥)
        /// </summary>
        int? DefaultActionBarTitleResource { get; }

        /// <summary>
        /// 在页面上查找到的 <see cref="Toolbar"/> 缓存字段
        /// </summary>
        Toolbar? ToolbarCache { get; set; }

        /// <summary>
        /// 当前活动的 <see cref="Toolbar"/>
        /// </summary>
        Toolbar Toolbar { get; }

        /// <summary>
        /// 可重写在页面上查找 <see cref="Toolbar"/>
        /// </summary>
        /// <returns></returns>
        Toolbar? FindToolbar();

        /// <summary>
        /// 绑定工具栏
        /// </summary>
        void BindingToolbar();

        string GetString(int resId);

        /// <inheritdoc cref="BindingToolbar"/>
        public static void BindingToolbar(IToolbar t)
        {
            if (!t.HasToolbar) return;

            string? defaultActionBarTitleString = t.DefaultActionBarTitleString;
            int? defaultActionBarTitleResource = t.DefaultActionBarTitleResource;

            AppCompatActivity? activity = t is AppCompatActivity a ? a : null;
            Fragment? fragment = t is Fragment f ? f : null;

            if (fragment != null && fragment.Activity is IToolbar t2 && t2.HasToolbar)
            {
                if (string.IsNullOrEmpty(defaultActionBarTitleString))
                {
                    defaultActionBarTitleString = t2.DefaultActionBarTitleString;
                }
                else if (!defaultActionBarTitleResource.HasValue)
                {
                    defaultActionBarTitleResource = t2.DefaultActionBarTitleResource;
                }
            }

            var hasTitleString = !string.IsNullOrEmpty(defaultActionBarTitleString);
            var hasTitleResource = defaultActionBarTitleResource.HasValue;
            if (hasTitleString || hasTitleResource)
            {
                t.ToolbarCache = t.FindToolbar() ?? t.FindViewById<Toolbar>(R.id.toolbar);
                if (t.ToolbarCache != null)
                {
                    string? title = null;
                    if (hasTitleString)
                    {
                        title = defaultActionBarTitleString;
                    }
                    else if (hasTitleResource)
                    {
#pragma warning disable CS8629 // 可为 null 的值类型可为 null。
                        var resId = defaultActionBarTitleResource.Value;
#pragma warning restore CS8629 // 可为 null 的值类型可为 null。
                        title = t.GetString(resId);
                    }

                    Action? navigationClick = null;

                    if (activity != null)
                    {
                        activity.SetSupportActionBar(t.ToolbarCache);
                        var actionBar = activity.SupportActionBar;
                        actionBar.SetDisplayHomeAsUpEnabled(true);
                        actionBar.SetHomeButtonEnabled(true);
                        if (title != null) actionBar.Title = title;
                        navigationClick = activity.OnBackPressed;
                    }
                    else if (fragment != null)
                    {
                        navigationClick = () => fragment.Activity?.OnBackPressed();
                    }

                    if (navigationClick != null)
                    {
                        t.ToolbarCache.NavigationClick += (_, _) => navigationClick();
                    }
                }
            }
        }

        /// <inheritdoc cref="Toolbar"/>
        public static Toolbar GetToolbar(IToolbar t)
            => t.ToolbarCache ?? throw new ArgumentNullException(nameof(ToolbarCache));
    }
}

namespace System.Application.UI.Activities
{
    partial class BaseActivity : IToolbar
    {
        /// <inheritdoc cref="IToolbar.HasToolbar"/>
        protected virtual bool HasToolbar { get; }

        bool IToolbar.HasToolbar => HasToolbar;

        /// <inheritdoc cref="IToolbar.DefaultActionBarTitleString"/>
        protected virtual string? DefaultActionBarTitleString { get; }

        string? IToolbar.DefaultActionBarTitleString => DefaultActionBarTitleString;

        /// <inheritdoc cref="IToolbar.DefaultActionBarTitleResource"/>
        protected virtual int? DefaultActionBarTitleResource { get; }

        int? IToolbar.DefaultActionBarTitleResource => DefaultActionBarTitleResource;

        /// <inheritdoc cref="IToolbar.ToolbarCache"/>
        Toolbar? toolbar;

        Toolbar? IToolbar.ToolbarCache { get => toolbar; set => toolbar = value; }

        /// <inheritdoc cref="IToolbar.Toolbar"/>
        protected Toolbar Toolbar => IToolbar.GetToolbar(this);

        Toolbar IToolbar.Toolbar => Toolbar;

        /// <inheritdoc cref="IToolbar.FindToolbar"/>
        protected virtual Toolbar? FindToolbar() => null;

        Toolbar? IToolbar.FindToolbar() => FindToolbar();

        /// <inheritdoc cref="IToolbar.BindingToolbar"/>
        protected virtual void BindingToolbar() => IToolbar.BindingToolbar(this);

        void IToolbar.BindingToolbar() => BindingToolbar();
    }
}

namespace System.Application.UI.Fragments
{
    partial class BaseFragment : IToolbar
    {
        /// <inheritdoc cref="IToolbar.HasToolbar"/>
        protected virtual bool HasToolbar { get; }

        bool IToolbar.HasToolbar => HasToolbar;

        /// <inheritdoc cref="IToolbar.DefaultActionBarTitleString"/>
        protected virtual string? DefaultActionBarTitleString { get; }

        string? IToolbar.DefaultActionBarTitleString => DefaultActionBarTitleString;

        /// <inheritdoc cref="IToolbar.DefaultActionBarTitleResource"/>
        protected virtual int? DefaultActionBarTitleResource { get; }

        int? IToolbar.DefaultActionBarTitleResource => DefaultActionBarTitleResource;

        /// <inheritdoc cref="IToolbar.ToolbarCache"/>
        Toolbar? toolbar;

        Toolbar? IToolbar.ToolbarCache { get => toolbar; set => toolbar = value; }

        /// <inheritdoc cref="IToolbar.Toolbar"/>
        protected Toolbar Toolbar => IToolbar.GetToolbar(this);

        Toolbar IToolbar.Toolbar => Toolbar;

        /// <inheritdoc cref="IToolbar.FindToolbar"/>
        protected virtual Toolbar? FindToolbar() => null;

        Toolbar? IToolbar.FindToolbar() => FindToolbar();

        /// <inheritdoc cref="IToolbar.BindingToolbar"/>
        protected virtual void BindingToolbar() => IToolbar.BindingToolbar(this);

        void IToolbar.BindingToolbar() => BindingToolbar();
    }
}