namespace System.Application.Services
{
    /// <inheritdoc cref="INavigationService"/>
    public interface IPlatformNavigationService : INavigationService
    {
        /// <summary>
        /// 将UI控制器类型转换为安卓原生活动类型
        /// </summary>
        /// <param name="uiControllerType"></param>
        /// <returns></returns>
        Type GetActivityType(Type uiControllerType);
    }
}