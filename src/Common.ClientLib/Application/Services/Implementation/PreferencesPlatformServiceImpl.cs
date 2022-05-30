using SQLite;
using System.Application.Entities;
using System.Application.Repositories;
using System.Diagnostics;
using System.Globalization;
using SQLiteNotNull = SQLite.NotNullAttribute;
using SQLiteTable = SQLite.TableAttribute;

namespace System.Application.Services.Implementation;

internal sealed class PreferencesPlatformServiceImpl : IPreferencesGenericPlatformService
{
    readonly SQLiteConnection conn;

    public PreferencesPlatformServiceImpl()
    {
        conn = Repository.GetDbConnectionSync<Entity>();
    }

    static string GetId(string key) => $"{key}_K";

    static string GetId(string key, string? sharedName)
    {
        if (sharedName == null) return GetId(key);
        return $"{key}_{sharedName}_S";
    }

    public void PlatformClear(string? sharedName)
    {
        conn.Execute(Sql_Delete_Where_SharedName_Equals, sharedName ?? string.Empty);
    }

    public bool PlatformContainsKey(string key, string? sharedName)
    {
        var id = GetId(key, sharedName);
        sharedName ??= string.Empty;
        var item = conn.Table<Entity>()
            .Where(x => x.Id == id && x.SharedName == sharedName)
            .FirstOrDefault();
        return item != null;
    }

    public T? PlatformGet<T>(string key, T? defaultValue, string? sharedName) where T : notnull, IConvertible
    {
        var id = GetId(key, sharedName);
        sharedName ??= string.Empty;
        var item = conn.Table<Entity>()
            .Where(x => x.Id == id && x.SharedName == sharedName)
            .FirstOrDefault();
        if (item == null) return defaultValue;
        var value = ConvertibleHelper.Convert<T>(item.Value);
        return value;
    }

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
                SharedName = sharedName ?? string.Empty,
            });
        }
    }

    const string TableName = "1984415E";
    const string ColumnName_Id = "0F5E4BAA";
    const string ColumnName_Value = "4FC331D7";
    const string ColumnName_SharedName = "F6A739AA";

    const string Sql_Delete_Where_SharedName_Equals =
        $"DELETE FROM \"{TableName}\" WHERE \"{ColumnName_SharedName}\" = ?";

    //const string Sql_Delete_Where_Id_Equals_And_SharedName_IsNull =
    //    $"DELETE FROM \"{TableName}\" WHERE \"{ColumnName_Id}\" = ? AND \"{ColumnName_SharedName}\" = NULL";

    //const string Sql_Delete_Where_Id_Equals_And_SharedName_Equals =
    //    $"DELETE FROM \"{TableName}\" WHERE \"{ColumnName_Id}\" = ? AND \"{ColumnName_SharedName}\" = ?";

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
        public string Id { get; set; } = string.Empty;

        [Column(ColumnName_Value)]
        [SQLiteNotNull]
        public string Value { get; set; } = string.Empty;
#pragma warning restore CS8618 // 在退出构造函数时，不可为 null 的字段必须包含非 null 值。请考虑声明为可以为 null。

        [Column(ColumnName_SharedName)]
        [SQLiteNotNull]
        public string SharedName { get; set; } = string.Empty;

        string DebuggerDisplay() => SharedName == string.Empty ? $"{Id}, {Value}" : $"{Id}, {Value}, {SharedName}";
    }
}
