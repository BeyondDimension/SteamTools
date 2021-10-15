using System;
using System.Application.Services;
using System.Collections.Generic;
using System.Text;
using System.Windows.Input;

// ReSharper disable once CheckNamespace
namespace System.Application.UI.ViewModels
{
    public class DialogWindowViewModel : WindowViewModel
    {
        public bool DialogResult { get; set; }

        public ICommand? OK { get; set; }

        public ICommand? Cancel { get; set; }

        /// <summary>
        /// 当点击确定时是否可以关闭窗口
        /// </summary>
        /// <returns></returns>
        public virtual bool OnOKClickCanClose()
        {
            if (this is ITextBoxWindowViewModel viewModel_tb)
            {
                if (!viewModel_tb.InputValidator()) // 文本框弹窗输入验证不通过时不能关闭
                {
                    return false;
                }
            }
            return true;
        }
    }
}
