using DBreeze;
using DBreeze.Utils;

namespace System.Application.Settings;

public static class SettingsProviderV3
{
    static DBreezeEngine? _Provider;
    static readonly object lock_obj = new();

    public static DBreezeEngine Provider
    {
        get
        {
            lock (lock_obj)
            {
                if (_Provider == null) Load();
                return _Provider ?? throw new ArgumentNullException(nameof(_Provider));
            }
        }
        set => _Provider = value;
    }

    public static void Load()
    {
        if (_Provider == null)
        {
            _Provider = new(Path.Combine(IOPath.AppDataDirectory, "Config"));

            //Setting up NetJSON serializer (from NuGet) to be used by DBreeze
            CustomSerializator.ByteArraySerializator = value => Serializable.SMP(value == null ? typeof(object) : value.GetType(), value!);
            CustomSerializator.ByteArrayDeSerializator = (buffer, type) => Serializable.DMP(type, buffer);
        }
    }

    /// <summary>
    /// 将数据从
    /// <para>使用 <see cref="IDictionary{TKey, TValue}"/> + <see cref="MessagePack"/> 单文件 的 FileSettingsProvider</para>    
    /// <para>迁移到</para>
    /// <para>使用 DBreeze 的 SettingsProviderV3</para>
    /// </summary>
    public static void Migrate()
    {
        var filePath = Path.Combine(IOPath.AppDataDirectory, "Config" + FileEx.MPO);
        bool exists = false;
        try
        {
            exists = File.Exists(filePath);
        }
        catch
        {

        }
        if (exists)
        {
            try
            {
                using var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite | FileShare.Delete);
                if (stream.Position > 0)
                {
                    stream.Position = 0;
                }
                var source = stream.Length == 0 ? null : Serializable.DMP<IDictionary<string, object?>>(stream);
                if (source.Any_Nullable())
                {
                    if (_Provider == null) Load();
                    using var t = Provider.GetTransaction();
                    foreach (var item in source)
                    {
                        if (item.Value == null) continue;
                        var valueType = item.Value.GetType();
                        var value = Serializable.SMP(valueType, item.Value);
                        t.Insert(ISerializableProperty.tableName, item.Key, value);
                    }
                    t.Commit();
                }
            }
            catch
            {

            }
            finally
            {
                IOPath.FileTryDelete(filePath);
            }
        }
    }
}
