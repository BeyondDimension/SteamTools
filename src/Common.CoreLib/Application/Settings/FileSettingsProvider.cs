#if !DBREEZE
using System.Diagnostics.CodeAnalysis;

namespace System.Application.Settings;

public sealed class FileSettingsProvider : ISerializationProvider
{
    private readonly string _path;
    private readonly object _sync = new();
    private SortedDictionary<string, object?> _settings = new();

    public bool IsLoaded { get; private set; }

    public FileSettingsProvider(string path)
    {
        _path = path;
    }

    public void SetValue<T>(string key, T value)
    {
        lock (_sync)
        {
            _settings[key] = value;
        }
    }

    public bool TryGetValue<T>(string key, [NotNullWhen(true)] out T? value) where T : notnull
    {
        lock (_sync)
        {
            if (_settings.TryGetValue(key, out object? obj) && obj is T t)
            {
                value = t;
                return true;
            }
        }

        value = default;
        return false;
    }

    public bool RemoveValue(string key)
    {
        lock (_sync)
        {
            return _settings.Remove(key);
        }
    }

    public void Save()
    {
        if (_settings.Count == 0) return;

        var dir = Path.GetDirectoryName(_path);
        if (dir == null) throw new DirectoryNotFoundException();

        if (!Directory.Exists(dir))
        {
            Directory.CreateDirectory(dir);
        }

        lock (_sync)
        {
            using var stream = OpenReadWrite(_path);
            if (stream.Position > 0)
            {
                stream.Position = 0;
            }
            var data = Serializable.SMP(_settings);
            stream.Write(data);
            //XamlServices.Save(stream, this._settings);
        }
    }

    static FileStream OpenReadWrite(string path)
    {
        try
        {
            return OpenReadWrite();
        }
        catch (Exception)
        {
            if (File.Exists(path))
            {
                if (IOPath.FileTryDelete(path))
                {
                    // 直接打开文件流失败，尝试删除后再打开一次
                    return OpenReadWrite();
                }
            }
            throw;
        }

        FileStream OpenReadWrite() => new(path, FileMode.Create, FileAccess.ReadWrite, FileShare.ReadWrite | FileShare.Delete);
    }

    public void Load()
    {
        if (File.Exists(_path))
        {
            using var stream = new FileStream(_path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite | FileShare.Delete);
            lock (_sync)
            {
                if (stream.Position > 0)
                {
                    stream.Position = 0;
                }
                var source = stream.Length == 0 ? null : Serializable.DMP<IDictionary<string, object?>>(stream);
                //var source = XamlServices.Load(stream) as IDictionary<string, object>;
                _settings = source == null
                    ? new SortedDictionary<string, object?>()
                    : new SortedDictionary<string, object?>(source);
            }
        }
        else
        {
            lock (_sync)
            {
                _settings = new SortedDictionary<string, object?>();
            }
        }

        IsLoaded = true;
    }

    public string ToJsonString() => Serializable.SJSON(_settings, writeIndented: true);

    event EventHandler ISerializationProvider.Reloaded
    {
        add { }
        remove { }
    }
}
#endif