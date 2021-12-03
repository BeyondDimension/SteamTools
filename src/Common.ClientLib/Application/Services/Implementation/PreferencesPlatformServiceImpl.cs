using SQLite;
using System.Application.Entities;
using System.Application.Repositories;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using NotNull = System.Diagnostics.CodeAnalysis.NotNullAttribute;
using SQLiteNotNull = SQLite.NotNullAttribute;
using SQLiteTable = SQLite.TableAttribute;

namespace System.Application.Services.Implementation
{
    internal sealed class PreferencesPlatformServiceImpl : IPreferencesPlatformService
    {
        readonly SQLiteConnection conn;

        public PreferencesPlatformServiceImpl()
        {
            conn = Repository.GetDbConnectionSync<Entity>();
        }

        static string GetId(string key, string? sharedName) => key + sharedName;

        public void PlatformClear(string? sharedName)
        {
            if (sharedName == null)
            {
                conn.Execute(Sql_Delete_All);
            }
            else
            {
                conn.Execute(Sql_Delete_Where_SharedName_Equals, sharedName);
            }
        }

        //public void PlatformClear(string? sharedName)
        //{
        //    if (sharedName == null)
        //    {
        //        conn.DeleteAll<Entity>();
        //    }
        //    else
        //    {
        //        var items = conn.Table<Entity>().Where(x => x.SharedName == sharedName).ToArray();
        //        foreach (var item in items)
        //        {
        //            conn.Delete(item);
        //        }
        //    }
        //}

        //public bool PlatformContainsKey(string key, string? sharedName)
        //{
        //    int r;
        //    if (sharedName == null)
        //    {
        //        r = conn.Execute(Sql_Select_SharedName_Where_Key_Equals_And_SharedName_IsNull, key);
        //    }
        //    else
        //    {
        //        r = conn.Execute(Sql_Select_SharedName_Where_Key_Equals_And_SharedName_Equals, GetId(key, sharedName), sharedName);
        //    }
        //    return r > 0;
        //}

        public bool PlatformContainsKey(string key, string? sharedName)
        {
            var id = GetId(key, sharedName);
            var item = conn.Table<Entity>().Where(x => x.Id == id && x.SharedName == sharedName).FirstOrDefault();
            return item != null;
        }

        //public T? PlatformGet<T>(string key, T? defaultValue, string? sharedName) where T : notnull, IConvertible
        //{
        //    string? r;
        //    if (sharedName == null)
        //    {
        //        r = conn.ExecuteScalar<string?>(Sql_Select_Value_Where_Key_Equals_And_SharedName_IsNull, key);
        //    }
        //    else
        //    {
        //        r = conn.ExecuteScalar<string?>(Sql_Select_Value_Where_Key_Equals_And_SharedName_Equals, GetId(key, sharedName), sharedName);
        //    }
        //    if (r == null) return defaultValue;
        //    var r2 = ConvertibleHelper.Convert<T>(r);
        //    return r2;
        //}

        public T? PlatformGet<T>(string key, T? defaultValue, string? sharedName) where T : notnull, IConvertible
        {
            var id = GetId(key, sharedName);
            var item = conn.Table<Entity>().Where(x => x.Id == id && x.SharedName == sharedName).FirstOrDefault();
            if (item == null) return defaultValue;
            var value = ConvertibleHelper.Convert<T>(item.Value);
            return value;
        }

        //public void PlatformRemove(string key, string? sharedName)
        //{
        //    if (sharedName == null)
        //    {
        //        conn.Execute(Sql_Delete_Where_Id_Equals_And_SharedName_IsNull, key);
        //    }
        //    else
        //    {
        //        conn.Execute(Sql_Delete_Where_Id_Equals_And_SharedName_Equals, GetId(key, sharedName), sharedName);
        //    }
        //}

        public void PlatformRemove(string key, string? sharedName)
        {
            var id = GetId(key, sharedName);
            conn.Delete<Entity>(id);
        }

        public void PlatformSet<T>(string key, T? value, string? sharedName) where T : notnull, IConvertible
        {
            if (value == null)
            {
                PlatformRemove(key, sharedName);
            }
            else
            {
                var id = GetId(key, sharedName);
                conn.InsertOrReplace(new Entity
                {
                    Id = id,
                    Value = value.ToString(CultureInfo.InvariantCulture),
                    SharedName = sharedName,
                });
            }
        }

        const string TableName = "0984415E";
        const string ColumnName_Id = "0F5E4BAA";
        const string ColumnName_Value = "4FC331D7";
        const string ColumnName_SharedName = "F6A739AA";

        const string Sql_Delete_All =
            $"DELETE FROM \"{TableName}\"";

        const string Sql_Delete_Where_SharedName_Equals =
            $"DELETE FROM \"{TableName}\" WHERE \"{ColumnName_SharedName}\" = ?";

        const string Sql_Delete_Where_Id_Equals_And_SharedName_IsNull =
            $"DELETE FROM \"{TableName}\" WHERE \"{ColumnName_Id}\" = ? AND \"{ColumnName_SharedName}\" = NULL";

        const string Sql_Delete_Where_Id_Equals_And_SharedName_Equals =
            $"DELETE FROM \"{TableName}\" WHERE \"{ColumnName_Id}\" = ? AND \"{ColumnName_SharedName}\" = ?";

        //const string Sql_Select_SharedName_Where_Key_Equals_And_SharedName_IsNull =
        //    $"SELECT \"{ColumnName_SharedName}\" FROM \"{TableName}\" WHERE \"{ColumnName_Id}\" = ? AND \"{ColumnName_SharedName}\" = NULL";

        //const string Sql_Select_SharedName_Where_Key_Equals_And_SharedName_Equals =
        //    $"SELECT \"{ColumnName_SharedName}\" FROM \"{TableName}\" WHERE \"{ColumnName_Id}\" = ? AND \"{ColumnName_SharedName}\" = ?";

        //const string Sql_Select_Value_Where_Key_Equals_And_SharedName_IsNull =
        //    $"SELECT \"{ColumnName_Value}\" FROM \"{TableName}\" WHERE \"{ColumnName_Id}\" = ? AND \"{ColumnName_SharedName}\" = NULL";

        //const string Sql_Select_Value_Where_Key_Equals_And_SharedName_Equals =
        //    $"SELECT \"{ColumnName_Value}\" FROM \"{TableName}\" WHERE \"{ColumnName_Id}\" = ? AND \"{ColumnName_SharedName}\" = ?";

        [SQLiteTable(TableName)]
        [DebuggerDisplay("{DebuggerDisplay(),nq}")]
        public sealed class Entity : IEntity<string>
        {
#pragma warning disable CS8618 // 在退出构造函数时，不可为 null 的字段必须包含非 null 值。请考虑声明为可以为 null。
            [Column(ColumnName_Id)]
            [PrimaryKey]
            [SQLiteNotNull]
            [NotNull, DisallowNull] // C# 8 not null
            public string? Id { get; set; }

            [Column(ColumnName_Value)]
            [SQLiteNotNull]
            [NotNull, DisallowNull] // C# 8 not null
            public string? Value { get; set; }

            [Column(ColumnName_SharedName)]
            [SQLiteNotNull]
            public string? SharedName { get; set; }
#pragma warning restore CS8618 // 在退出构造函数时，不可为 null 的字段必须包含非 null 值。请考虑声明为可以为 null。

            string DebuggerDisplay() => SharedName == null ? $"{Id}, {Value}" : $"{Id}, {Value}, {SharedName}";
        }
    }
}
