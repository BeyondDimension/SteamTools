namespace System.Application.Services
{
    /// <summary>
    /// 页面跳转导航服务
    /// </summary>
    public interface INavigationService
    {
        /// <summary>
        /// 页面跳转
        /// </summary>
        /// <param name="type"></param>
        /// <param name="flags"></param>
        void Push(Type type, PushFlags flags = PushFlags.Empty);

        /// <summary>
        /// 后退
        /// </summary>
        void Pop();
    }
}