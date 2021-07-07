namespace System.Application.UI.ViewModels
{
    public interface ITextBoxWindowViewModel
    {
        string? Value { get; set; }

        /// <summary>
        /// 输入内容验证
        /// </summary>
        /// <returns></returns>
        bool InputValidator() => true;
    }
}