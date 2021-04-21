namespace System.Application.Services
{
    /// <summary>
    /// Toast属于一种轻量级的反馈，常常以小弹框的形式出现，一般出现2秒或3.5秒后会自动消失，可以出现在屏幕上中下任意位置，但同个产品会模块尽量使用同一位置，让用户产生统一认知
    /// </summary>
    public interface IToast
    {
        /// <summary>
        /// 显示Toast
        /// </summary>
        /// <param name="text"></param>
        /// <param name="duration"></param>
        void Show(string text, int? duration = null);

        /// <inheritdoc cref="Show(string, int?)"/>
        void Show(string text, ToastLength duration);
    }
}