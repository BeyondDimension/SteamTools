// ReSharper disable once CheckNamespace
namespace BD.WTTS.UI.ViewModels;

public class DialogWindowViewModel : WindowViewModel
{
    public bool DialogResult { get; set; }

    [IgnoreDataMember, MPIgnore, MP2Ignore, N_JsonIgnore, S_JsonIgnore]
    public ICommand? OK { get; set; }

    [IgnoreDataMember, MPIgnore, MP2Ignore, N_JsonIgnore, S_JsonIgnore]
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
