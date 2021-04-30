using Android.App;
using AndroidX.AppCompat.App;

namespace System.Application.UI.Fragments
{
    /// <summary>
    /// 对话框片段(DialogFragment)
    /// </summary>
    public interface IDialogFragment : IFragment
    {
        new AppCompatDialogFragment Fragment { get; }

        /// <summary>
        /// 返回此Fragment当前控制的对话框
        /// </summary>
        Dialog Dialog { get; }

        /// <summary>
        /// 控制显示的对话框是否可取消
        /// </summary>
        bool Cancelable { get; set; }

        /// <summary>
        /// 关闭片段及其对话框
        /// <para>如果该片段被添加到后栈，则将弹出该条目之前（包括该条目）的所有后栈状态</para>
        /// <para>否则，将提交一个新事务来删除片段</para>
        /// </summary>
        void Dismiss();

        /// <summary>
        /// 使用 <see cref="FragmentTransaction.CommitAllowingStateLoss"/> 的 <see cref="Dismiss"/> 版本
        /// </summary>
        void DismissAllowingStateLoss();
    }

    partial class BaseDialogFragment : IDialogFragment
    {
        AppCompatDialogFragment IDialogFragment.Fragment => this;
    }

    partial class BaseBottomSheetDialogFragment : IDialogFragment
    {
        AppCompatDialogFragment IDialogFragment.Fragment => this;
    }
}