using ReactiveUI;
using System.Application.Services;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;

// ReSharper disable once CheckNamespace
namespace System.Application.UI.ViewModels
{
    /// <summary>
    /// 文件资源管理器(目录浏览)视图模型
    /// </summary>
    public class ExplorerPageViewModel : PageViewModel
    {
        public ExplorerPageViewModel()
        {
            title = DefaultTitle;
        }

        string _CurrentPath = string.Empty;
        public string CurrentPath
        {
            get => _CurrentPath;
            set
            {
                if (_CurrentPath == value) return;
                _CurrentPath = value;
                var title = DefaultTitle;
                foreach (var item in GetRootPaths())
                {
                    if (_CurrentPath.StartsWith(item.Key))
                    {
                        title = Path.DirectorySeparatorChar +
                            item.Value +
                            _CurrentPath.TrimStart(item.Key);
                        break;
                    }
                }
                Title = title;
                PathInfos.Clear();
                if (string.IsNullOrEmpty(_CurrentPath))
                {
                    PathInfos.AddRange(GetRootPathInfoViewModels<List<PathInfoViewModel>>());
                }
                else
                {
                    AddRange(PathInfos, _CurrentPath);
                }
            }
        }

        static readonly string DefaultTitle = Path.DirectorySeparatorChar.ToString();

        public ObservableCollection<PathInfoViewModel> PathInfos { get; } = GetRootPathInfoViewModels<ObservableCollection<PathInfoViewModel>>();

        public class PathInfoViewModel : ReactiveObject
        {
            public PathInfoViewModel(FileSystemInfo fileSystemInfo, string? name = null)
            {
                Name = name ?? fileSystemInfo.Name;
                FullName = fileSystemInfo.FullName;
                Desc = $" | {fileSystemInfo.CreationTime.ToString(DateTimeFormat.Standard)}";
                if (fileSystemInfo is FileInfo fileInfo)
                {
                    IsDirectory = false;
                    var fileSize = IOPath.GetSizeString(fileInfo.Length);
                    Desc = $"Size: {fileSize}{Desc}";
                }
                else if (fileSystemInfo is DirectoryInfo dirInfo)
                {
                    IsDirectory = true;
                    var ignore_rule_log_under_cache_on_cache = /*IApplication.LogUnderCache &&*/ dirInfo.FullName == IOPath.CacheDirectory;
                    var filesCount = dirInfo.GetFiles().Length;
                    var dirsCount = dirInfo.GetDirectories().Length;
                    Desc = $"Count: {filesCount + (ignore_rule_log_under_cache_on_cache ? dirsCount - 1 : dirsCount)}{Desc}";
                }
                else
                {
                    throw new NotSupportedException();
                }
            }

            public bool IsDirectory { get; }

            public string Name { get; }

            public string FullName { get; }

            public string Desc { get; }
        }

        static IEnumerable<KeyValuePair<string, string>> GetRootPaths()
        {
            yield return new(IOPath.AppDataDirectory, IOPath.DirName_AppData);
            yield return new(IApplication.LogDirPath, IApplication.LogDirName);
            yield return new(IOPath.CacheDirectory, IOPath.DirName_Cache);
        }

        static T GetRootPathInfoViewModels<T>() where T : ICollection<PathInfoViewModel>, new()
        {
            return new T()
            {
                new PathInfoViewModel(new DirectoryInfo(IOPath.AppDataDirectory), IOPath.DirName_AppData),
                new PathInfoViewModel(new DirectoryInfo(IOPath.CacheDirectory), IOPath.DirName_Cache),
                new PathInfoViewModel(new DirectoryInfo(IApplication.LogDirPath)),
            };
        }

        public static void AddRange(IList<PathInfoViewModel> list, string dirPath)
        {
            var ignore_rule_log_under_cache_on_cache = /*IApplication.LogUnderCache &&*/ dirPath == IOPath.CacheDirectory;
            var dirInfo = new DirectoryInfo(dirPath);
            Array.ForEach(dirInfo.GetDirectories(), x =>
            {
                if (ignore_rule_log_under_cache_on_cache && x.Name == IApplication.LogDirName) return;
                list.Add(new(x));
            });
            Array.ForEach(dirInfo.GetFiles(), x => list.Add(new(x)));
        }

        public void OnItemClick(PathInfoViewModel pathInfo)
        {
            if (pathInfo.IsDirectory)
            {
                CurrentPath = pathInfo.FullName;
            }
            else
            {
                var extension = Path.GetExtension(pathInfo.FullName);
                if (FileEx.IsSupportedTextReader(extension))
                {
                    IPlatformService.Instance.OpenFileByTextReader(pathInfo.FullName);
                }
            }
        }

        public bool OnBack()
        {
            if (Title != DefaultTitle)
            {
                var dirs = Title.Split(Path.DirectorySeparatorChar, StringSplitOptions.RemoveEmptyEntries);
                if (dirs.Length == 1)
                {
                    CurrentPath = string.Empty;
                }
                else if (dirs.Length > 0)
                {
                    foreach (var item in GetRootPaths())
                    {
                        if (item.Value == dirs[0])
                        {
                            CurrentPath = Path.Combine(new[] { item.Key }.Concat(dirs.Skip(1).Take(dirs.Length - 2)).ToArray());
                            break;
                        }
                    }
                }
                else
                {
                    return false;
                }
                return true;
            }
            return false;
        }
    }
}