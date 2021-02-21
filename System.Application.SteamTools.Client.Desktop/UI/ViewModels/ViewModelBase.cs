using ReactiveUI;

namespace System.Application.UI.ViewModels
{
    public class ViewModelBase : ReactiveObject, ILocalizationViewModel
    {
        public ViewModelBase()
        {
            ILocalizationViewModel localization = this;
            localization.OnChangeLanguage();
        }

        /// <summary>
        /// 是否在设计器的上下文中运行
        /// </summary>
        public static bool IsInDesignMode { get; set; }

        protected static TViewModel GetViewModel<TViewModel>() where TViewModel : ViewModelBase, new()
        {
            if (IsInDesignMode) return new TViewModel();
            return DI.Get<TViewModel>();
        }
    }
}