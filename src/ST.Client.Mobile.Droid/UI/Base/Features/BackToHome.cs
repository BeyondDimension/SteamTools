namespace System.Application.UI.Activities
{
    partial class BaseActivity
    {
        /// <summary>
        /// 按 Back 键是否模拟 Home 键按下效果，也就是回到 Launcher，默认值 <see langword="false"/>
        /// </summary>
        protected virtual bool BackToHome { get; }
    }

    partial class BaseActivity<TViewBinding, TViewModel>
    {
        /// <summary>
        /// 按 Back 键是否模拟 Home 键按下效果，也就是回到 Launcher，默认值 <see langword="false"/>
        /// </summary>
        protected virtual bool BackToHome { get; }
    }
}