namespace System.Application.Services
{
    /// <summary>
    /// Toast显示拦截
    /// </summary>
    public interface IToastIntercept
    {
        /// <summary>
        /// 显示Toast消息前拦截
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        bool OnShowExecuting(string text);
    }
}