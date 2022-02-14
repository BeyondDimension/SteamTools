namespace System.Application.UI.Adapters
{
#pragma warning disable IDE1006 // 命名样式
    /// <inheritdoc cref="ItemClickEventArgs{TViewModel}"/>
    public interface ItemClickEventArgs
    {
        /// <summary>
        /// 位置
        /// </summary>
        int Position { get; }

        /// <summary>
        /// 当前项
        /// </summary>
        object Current { get; }

        bool Handled { get; set; }
    }

    /// <summary>
    /// 列表项点击事件参数
    /// </summary>
    /// <typeparam name="TViewModel"></typeparam>
    public interface ItemClickEventArgs<TViewModel> : ItemClickEventArgs
    {
        /// <inheritdoc cref="ItemClickEventArgs.Current"/>
        new TViewModel Current { get; }

        object ItemClickEventArgs.Current => Current;
    }
#pragma warning restore IDE1006 // 命名样式
}