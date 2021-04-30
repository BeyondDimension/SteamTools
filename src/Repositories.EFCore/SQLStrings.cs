namespace System
{
    public static partial class SQLStrings
    {
        public const string SqlServer = "Microsoft.EntityFrameworkCore.SqlServer";

        /// <summary>
        /// 返回包含计算机的日期和时间的 datetimeoffset(7) 值，SQL Server 的实例正在该计算机上运行。 时区偏移量包含在内。
        /// <para>https://docs.microsoft.com/zh-cn/sql/t-sql/functions/sysdatetimeoffset-transact-sql?view=azuresqldb-current</para>
        /// </summary>
        public const string SYSDATETIMEOFFSET = "SYSDATETIMEOFFSET()";

        /// <summary>
        /// 在启动 Windows 后在指定计算机上创建大于先前通过该函数生成的任何 GUID 的 GUID。 在重新启动 Windows 后，GUID 可以再次从一个较低的范围开始，但仍是全局唯一的。 在 GUID 列用作行标识符时，使用 NEWSEQUENTIALID 可能比使用 NEWID 函数的速度更快。 其原因在于，NEWID 函数导致随机行为并且使用更少的缓存数据页。 使用 NEWSEQUENTIALID 还有助于完全填充数据和索引页。
        /// <para>https://docs.microsoft.com/zh-cn/sql/t-sql/functions/newsequentialid-transact-sql?view=azuresqldb-current</para>
        /// </summary>
        public const string NEWSEQUENTIALID = "newsequentialid()";

        /// <summary>
        /// 允许将显式值插入到表的标识列中。
        /// <para>https://docs.microsoft.com/zh-cn/sql/t-sql/statements/set-identity-insert-transact-sql?view=azuresqldb-current</para>
        /// </summary>
        /// <param name="tableName"></param>
        /// <param name="enable"></param>
        /// <returns></returns>
        public static string IDENTITY_INSERT(string tableName, bool enable)
        {
            var value = enable ? ON : OFF;
            var sql = $"SET IDENTITY_INSERT {tableName} {value}";
            return sql;
        }

        public const string ON = "ON";

        public const string OFF = "OFF";
    }
}