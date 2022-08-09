using SQLite;
using System.Application.Entities;
using System.Application.Repositories;
using System.Application.Settings;
using System.Diagnostics;
using System.Globalization;
using SQLiteNotNull = SQLite.NotNullAttribute;
using SQLiteTable = SQLite.TableAttribute;

namespace System.Application.Services.Implementation;

#if DBREEZE
partial class PreferencesPlatformServiceImplV2
#else
sealed class PreferencesPlatformServiceImpl : IPreferencesGenericPlatformService
#endif
{
#if !DBREEZE
    readonly SQLiteConnection conn;

    public PreferencesPlatformServiceImpl()
    {
        conn = Repository.GetDbConnectionSync<Entity>();
    }

    internal static string GetId(string key) => $"{key}_K";

    internal static string GetId(string key, string? sharedName)
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
#endif

    const string TableName = "1984415E";
    const string ColumnName_Id = "0F5E4BAA";
    const string ColumnName_Value = "4FC331D7";
    const string ColumnName_SharedName = "F6A739AA";

#if !DBREEZE
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
#endif

    [SQLiteTable(TableName)]
    [DebuggerDisplay("{DebuggerDisplay(),nq}")]
    public sealed class Entity : IEntity<string>
    {
        [Column(ColumnName_Id)]
        [PrimaryKey]
        [SQLiteNotNull]
        public string Id { get; set; } = string.Empty;

        [Column(ColumnName_Value)]
        [SQLiteNotNull]
        public string Value { get; set; } = string.Empty;

        [Column(ColumnName_SharedName)]
        [SQLiteNotNull]
        public string SharedName { get; set; } = string.Empty;

        string DebuggerDisplay() => SharedName == string.Empty ? $"{Id}, {Value}" : $"{Id}, {Value}, {SharedName}";
    }
}

#if DBREEZE
public sealed partial class PreferencesPlatformServiceImplV2 : IPreferencesGenericPlatformService
{
    static string GetTableName(string? sharedName) => $"Preferences.{sharedName}";

    public void PlatformSet<T>(string key, T? value, string? sharedName) where T : notnull, IConvertible
    {
        if (value == null)
        {
            PlatformRemove(key, sharedName);
        }
        else
        {
            using var t = SettingsProviderV3.Provider.GetTransaction();
            t.Insert(GetTableName(sharedName), key, value.ToString(CultureInfo.InvariantCulture));
            t.Commit();
        }
    }

    public void PlatformClear(string? sharedName)
    {
        using var t = SettingsProviderV3.Provider.GetTransaction();
        t.RemoveAllKeys(GetTableName(sharedName), true);
        t.Commit();
    }

    public bool PlatformContainsKey(string key, string? sharedName)
    {
        using var t = SettingsProviderV3.Provider.GetTransaction();
        var row = t.Select<string, string>(GetTableName(sharedName), key);
        if (row == null) return false;
        return row.Exists;
    }

    public T? PlatformGet<T>(string key, T? defaultValue, string? sharedName) where T : notnull, IConvertible
    {
        using var t = SettingsProviderV3.Provider.GetTransaction();
        var row = t.Select<string, string>(GetTableName(sharedName), key);
        if (row == null || !row.Exists) return defaultValue;
        var value = ConvertibleHelper.Convert<T>(row.Value);
        return value;
    }

    public void PlatformRemove(string key, string? sharedName)
    {
        using var t = SettingsProviderV3.Provider.GetTransaction();
        t.RemoveKey(GetTableName(sharedName), key);
        t.Commit();
    }

    /// <summary>
    /// 将数据从
    /// <para>使用 SQLiteConnection 的 PreferencesPlatformServiceImpl</para>    
    /// <para>迁移到</para>
    /// <para>使用 DBreeze 的 PreferencesPlatformServiceImplV2</para>
    /// </summary>
    public static void Migrate()
    {
        var dbPath = Repository.DataBaseDirectory;
        bool isOK = false;
        bool exists = false;
        try
        {
            var dbFilePath = Path.Combine(dbPath, "application2.dbf");
            try
            {
                exists = File.Exists(dbFilePath);
            }
            catch
            {

            }
            if (!exists) return;
            var conn = new SQLiteConnection(dbPath, SQLiteOpenFlags.ReadWrite | SQLiteOpenFlags.Create | SQLiteOpenFlags.SharedCache);
            var items = conn.Table<Entity>().ToArray();
            if (items.Any())
            {
                var itemGroups = items.GroupBy(x => x.SharedName).ToArray();
                foreach (var itemGroup in itemGroups)
                {
                    using var t = SettingsProviderV3.Provider.GetTransaction();
                    foreach (var item in itemGroup)
                    {
                        if (item.Value != null)
                        {
                            string key;
                            if (string.IsNullOrEmpty(item.SharedName)) key = item.Id.TrimEnd("_K");
                            else key = item.Id.TrimEnd($"_{item.SharedName}_S");
                            var tableName = GetTableName(item.SharedName);
                            t.Insert(tableName, key, item.Value.ToString(CultureInfo.InvariantCulture));
                        }
                    }
                    t.Commit();
                }
            }
            conn.Close();
            conn.Dispose();
            isOK = true;
        }
        catch
        {

        }
        finally
        {
            if (isOK)
            {
                var files = Directory.GetFiles(dbPath, "application2*");
                foreach (var item in files)
                {
                    IOPath.FileTryDelete(item);
                }
            }
        }
    }
}
#endif
