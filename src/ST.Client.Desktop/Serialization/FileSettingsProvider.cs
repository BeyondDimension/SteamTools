using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;

namespace System.Application.Serialization
{
    public class FileSettingsProvider : ISerializationProvider
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
                using var stream = new FileStream(_path, FileMode.Create, FileAccess.ReadWrite, FileShare.ReadWrite | FileShare.Delete);
                if (stream.Position > 0)
                {
                    stream.Position = 0;
                }
                var data = Serializable.SMP(_settings);
                stream.Write(data, 0, data.Length);
                //XamlServices.Save(stream, this._settings);
            }
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
                    var source = Serializable.DMP<IDictionary<string, object?>>(stream);
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

        event EventHandler ISerializationProvider.Reloaded
        {
            add { }
            remove { }
        }
    }
}