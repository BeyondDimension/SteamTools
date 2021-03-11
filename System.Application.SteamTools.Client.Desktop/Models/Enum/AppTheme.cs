namespace System.Application.Models
{
    /// <summary>
    /// 应用程序主题
    /// </summary>
    public enum AppTheme : byte
    {
        /// <summary>
        /// 跟随系统(目前还需要实现监听系统设置实现同步切换)
        /// </summary>
        FollowingSystem = 3,

        /// <summary>
        /// 亮色主题
        /// </summary>
        Light = 0,

        /// <summary>
        /// 暗色主题
        /// </summary>
        Dark = 1,

        /// <summary>
        /// 自定义主题
        /// </summary>
        Custom = 2,
    }
}