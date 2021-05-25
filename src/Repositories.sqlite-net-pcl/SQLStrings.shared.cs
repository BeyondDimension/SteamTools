using System.Application.Columns;
using System.Runtime.CompilerServices;

namespace System
{
    /// <summary>
    /// SQL语句字符串常量，使用字符串拼接时应注意防止 SQL注入攻击
    /// <para>EFCore 可采用 <see cref="FormattableString"/> 参数化传入参数</para>
    /// </summary>
    public static partial class SQLStrings
    {
        /// <summary>
        /// "select * from "
        /// <para>查询所有列</para>
        /// </summary>
        public const string SelectALLFrom = "select * from ";

        /// <summary>
        /// "select count(Id) from "
        /// <para>查询总数，仅Id列</para>
        /// </summary>
        public const string SelectCountFrom = "select count(Id) from ";

        /// <summary>
        /// "select count({0}) from "
        /// <para>查询总数，仅某一列</para>
        /// <para>注意：参数仅使用列名固定值，不可传递变量，防止SQL注入攻击</para>
        /// </summary>
        public const string SelectCountFrom_0 = "select count({0}) from ";

        /// <summary>
        /// " order by CreationTime desc"
        /// <para>根据 创建时间 倒序</para>
        /// </summary>
        public const string OrderByCreationTimeDescending = " order by CreationTime desc";

        /// <summary>
        /// " order by ReceiveTime, CreationTime desc"
        /// <para>根据 接收时间, 创建时间 倒序</para>
        /// </summary>
        public const string OrderByReceiveTimeThenByCreationTimeDescending = " order by ReceiveTime, CreationTime desc";

        /// <summary>
        /// " order by {0} desc"
        /// <para>根据 某一列 倒序</para>
        /// <para>注意：参数仅使用列名固定值，不可传递变量，防止SQL注入攻击</para>
        /// </summary>
        public const string OrderBy_Descending_0 = " order by {0} desc";

        /// <summary>
        /// "delete from "
        /// </summary>
        public const string DeleteFrom = "delete from ";

        /// <summary>
        /// " where Id = {0}"
        /// <para>注意：参数仅使用列名固定值，不可传递变量，防止SQL注入攻击</para>
        /// </summary>
        public const string WhereIdEqual_0 = " where Id = {0}";

        /// <summary>
        /// "delete from {tableName} where Id = {0}"
        /// </summary>
        /// <param name="tableName"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        public static FormattableString DeleteFromTableNameWhereIdEqual(string tableName, object id)
        {
            var sql = DeleteFrom + tableName + WhereIdEqual_0;
            return FormattableStringFactory.Create(sql, id);
        }

        /// <summary>
        /// "update from {tableName} set IsSoftDeleted = 1 where Id = {0}"
        /// </summary>
        /// <param name="tableName"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        public static FormattableString SoftDeleteFromTableNameWhereIdEqual(string tableName, object id)
        {
            var sql = Update + tableName + " set " + nameof(ISoftDeleted.SoftDeleted) + " = 1" + WhereIdEqual_0;
            return FormattableStringFactory.Create(sql, id);
        }

        /// <summary>
        /// "update "
        /// </summary>
        public const string Update = "update ";

        /// <summary>
        /// " where Id in ("
        /// </summary>
        public const string WhereInIds_ = " where Id in (";

        /// <summary>
        /// 右括号
        /// </summary>
        public const char RightBracket = ')';
    }
}