// ReSharper disable once CheckNamespace
namespace System.Windows
{
    /// <summary>
    /// 指定是否可以调整窗口大小，并在可调整大小的情况下指定如何调整窗口大小。由 SetResizeMode 使用。
    /// </summary>
    public enum ResizeModeCompat
    {
        /// <summary>
        /// 用户无法调整窗口的大小。 不显示 "最大化" 和 "最小化" 框。
        /// </summary>
        NoResize,

        /// <summary>
        /// 用户只能最小化窗口并从任务栏还原。 同时显示 "最小化" 和 "最大化" 框，但只启用 "最小化" 框。
        /// </summary>
        CanMinimize,

        /// <summary>
        /// 用户可以通过使用 "最小化" 和 "最大化" 框以及围绕窗口的可拖动轮廓来完全调整窗口的大小。 显示和启用 "最小化" 和 "最大化" 框。 (默认) 。
        /// </summary>
        CanResize,

        /// <summary>
        /// 此选项具有与相同的功能 <see cref="CanResize"/> ，但会将 "调整手柄" 添加到窗口的右下角。
        /// </summary>
        [Obsolete("not implemented")]
        CanResizeWithGrip,
    }
}