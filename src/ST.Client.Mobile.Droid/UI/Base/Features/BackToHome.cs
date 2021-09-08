// 通过重写 BackToHome => true; 实现 按 Back 键是否模拟 Home 键按下效果，也就是回到 Launcher

// ReSharper disable once CheckNamespace
namespace System.Application.UI.Activities
{
    partial class BaseActivity : IOnBackPressedCallback
    {
        /// <summary>
        /// 按 Back 键是否模拟 Home 键按下效果，也就是回到 Launcher，默认值 <see langword="false"/>
        /// </summary>
        protected virtual bool BackToHome { get; }

        public Func<bool>? HandleOnBackPressed { get; set; }
    }

    partial class BaseActivity<TViewBinding, TViewModel> : IOnBackPressedCallback
    {
        /// <summary>
        /// 按 Back 键是否模拟 Home 键按下效果，也就是回到 Launcher，默认值 <see langword="false"/>
        /// </summary>
        protected virtual bool BackToHome { get; }

        public Func<bool>? HandleOnBackPressed { get; set; }
    }

    public interface IOnBackPressedCallback
    {
        Func<bool>? HandleOnBackPressed { get; set; }
    }
}