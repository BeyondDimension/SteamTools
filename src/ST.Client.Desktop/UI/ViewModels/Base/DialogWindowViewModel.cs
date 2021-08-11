using System.Windows.Input;
#if __MOBILE__
using WindowViewModel = System.Application.UI.ViewModels.PageViewModel;
#endif

namespace System.Application.UI.ViewModels
{
    public class DialogWindowViewModel : WindowViewModel
    {
        public bool DialogResult { get; set; }

        public ICommand? OK { get; set; }

        public ICommand? Cancel { get; set; }

        /// <summary>
        /// 当前是否可以关闭窗口
        /// </summary>
        /// <returns></returns>
        public virtual bool CanClose()
        {
            if (this is ITextBoxWindowViewModel viewModel_p)
            {
                if (DialogResult)
                {
                    if (!viewModel_p.InputValidator()) // 文本框弹窗输入验证不通过时不能关闭
                    {
                        return false;
                    }
                }
            }
            return true;
        }
    }
}