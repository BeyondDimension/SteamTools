namespace System.Application.UI.Adapters
{
    /// <summary>
    /// 列表项点击事件参数
    /// </summary>
    /// <typeparam name="TViewModel"></typeparam>
#pragma warning disable IDE1006 // 命名样式
    public interface ItemClickEventArgs<TViewModel>
#pragma warning restore IDE1006 // 命名样式
    {
        /// <summary>
        /// 位置
        /// </summary>
        int Position { get; }

        /// <summary>
        /// 当前项
        /// </summary>
        TViewModel Current { get; }

        bool Handled { get; set; }
    }
}