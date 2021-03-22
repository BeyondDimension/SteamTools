namespace System.Application.Repositories
{
    /// <summary>
    /// 数据库单行执行结果
    /// </summary>
    public enum DbRowExecResult : byte
    {
        /// <summary>
        /// 无操作
        /// </summary>
        None = 0,

        /// <summary>
        /// 更新
        /// </summary>
        Update,

        /// <summary>
        /// 插入
        /// </summary>
        Insert,

        /// <summary>
        /// 参数不正确
        /// </summary>
        IncorrectArgument,

        /// <summary>
        /// 删除
        /// </summary>
        Delete,

        /// <summary>
        /// 插入或更新
        /// </summary>
        InsertOrUpdate,
    }

#if DEBUG

    [Obsolete("use DbRowExecResult", true)]
    public enum DatabaseLogic { }

#endif
}