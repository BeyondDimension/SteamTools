using ArchiSteamFarm;
using ICSharpCode.SharpZipLib.GZip;
using ICSharpCode.SharpZipLib.Tar;
using ICSharpCode.SharpZipLib.Zip.Compression;
using ReactiveUI;
using System.Application.Services;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using _ThisAssembly = System.Properties.ThisAssembly;

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

        public ExplorerPageViewModel(string rootPath)
        {
            CurrentAbsolutePath = rootPath;
            DefaultTitle = Title;
        }

        void RefreshCore(bool isChangePath)
        {
            PathInfos.Clear();
            if (string.IsNullOrEmpty(_CurrentAbsolutePath))
            {
                PathInfos.AddRange(GetRootPathInfoViewModels<List<PathInfoViewModel>>());
            }
            else
            {
                AddRange(PathInfos, _CurrentAbsolutePath);

                if (isChangePath)
                {
                    if (_CurrentAbsolutePath.StartsWith(SharedInfo.ConfigDirectory) ||
                     _CurrentAbsolutePath.StartsWith(SharedInfo.PluginsDirectory))
                    {
                        // 允许添加与删除的目录
                        IsSupportFileDelete = IsSupportFileCreate = true;
                    }
                    else if (_CurrentAbsolutePath.StartsWith(IOPath.CacheDirectory) ||
                        //_CurrentAbsolutePath.StartsWith(SharedInfo.HomeDirectory) ||
                        _CurrentAbsolutePath.StartsWith(IApplication.LogDirPath))
                    {
                        // 允许删除的目录
                        IsSupportFileDelete = true;
                    }
                    else
                    {
                        IsSupportFileDelete = IsSupportFileCreate = false;
                    }
                }
            }
        }

        public void Refresh() => RefreshCore(isChangePath: false);

        string _CurrentAbsolutePath = string.Empty;
        public string CurrentAbsolutePath
        {
            get => _CurrentAbsolutePath;
            set
            {
                if (_CurrentAbsolutePath == value) return;
                _CurrentAbsolutePath = value;
                var title = DefaultTitle;
                foreach (var item in GetRootPaths())
                {
                    if (_CurrentAbsolutePath.StartsWith(item.Key))
                    {
                        title = Path.DirectorySeparatorChar +
                            item.Value +
                            _CurrentAbsolutePath.TrimStart(item.Key);
                        break;
                    }
                }
                Title = title;
                RefreshCore(isChangePath: true);
            }
        }

        bool _IsSupportFileCreate;
        /// <summary>
        /// 当前目录是否支持创建或导入文件
        /// </summary>
        public bool IsSupportFileCreate
        {
            get => _IsSupportFileCreate;
            set => this.RaiseAndSetIfChanged(ref _IsSupportFileCreate, value);
        }

        bool _IsSupportFileDelete;
        /// <summary>
        /// 当前目录是否支持删除文件
        /// </summary>
        public bool IsSupportFileDelete
        {
            get => _IsSupportFileDelete;
            set => this.RaiseAndSetIfChanged(ref _IsSupportFileDelete, value);
        }

        bool _IsEditMode;
        /// <summary>
        /// 是否为编辑模式
        /// </summary>
        public bool IsEditMode
        {
            get => _IsEditMode;
            set => this.RaiseAndSetIfChanged(ref _IsEditMode, value);
        }

        public string DefaultTitle { get; set; } = Path.DirectorySeparatorChar.ToString();

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

            bool _Checked;
            public bool Checked
            {
                get => _Checked;
                set => this.RaiseAndSetIfChanged(ref _Checked, value);
            }
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
            if (IsEditMode)
            {
                pathInfo.Checked = !pathInfo.Checked;
                return;
            }
            if (pathInfo.IsDirectory)
            {
                CurrentAbsolutePath = pathInfo.FullName;
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

        public void OnItemLongClick(PathInfoViewModel pathInfo)
        {
            if (IsSupportFileDelete)
            {
                pathInfo.Checked = IsEditMode = true;
            }
        }

        public bool OnBack()
        {
            if (IsEditMode)
            {
                IsEditMode = false;
                return true;
            }
            if (Title != DefaultTitle)
            {
                var dirs = Title.Split(Path.DirectorySeparatorChar, StringSplitOptions.RemoveEmptyEntries);
                if (dirs.Length == 1)
                {
                    CurrentAbsolutePath = string.Empty;
                }
                else if (dirs.Length > 0)
                {
                    foreach (var item in GetRootPaths())
                    {
                        if (item.Value == dirs[0])
                        {
                            CurrentAbsolutePath = Path.Combine(new[] { item.Key }.Concat(dirs.Skip(1).Take(dirs.Length - 2)).ToArray());
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

        bool _InOperation;
        /// <summary>
        /// 是否正在操作中
        /// </summary>
        public bool InOperation
        {
            get => _InOperation;
            set => this.RaiseAndSetIfChanged(ref _InOperation, value);
        }

        /// <summary>
        /// 导入一组文件(多选)
        /// </summary>
        public async void ImportFiles()
        {
            if (!IsSupportFileCreate || InOperation) return;

            var files = await FilePicker2.PickMultipleAsync();
            if (files.Any_Nullable())
            {
                InOperation = true;

                foreach (var file in files)
                {
                    using var sourceStream = await file.OpenReadAsync();
                    var destFilePath = Path.Combine(_CurrentAbsolutePath, file.FileName);
                    using var destStream = File.Open(destFilePath, FileMode.OpenOrCreate, FileAccess.Write, FileShare.ReadWrite | FileShare.Delete);
                    destStream.Position = 0;
                    await sourceStream.CopyToAsync(destStream);
                    destStream.SetLength(destStream.Position);
                }
                Refresh();

                InOperation = false;
            }
        }

        /// <summary>
        /// 删除一组路径
        /// </summary>
        public void DeletePaths()
        {
            if (!IsSupportFileDelete || InOperation) return;

            InOperation = true;

            foreach (var item in PathInfos)
            {
                if (!item.Checked) continue;

                if (item.IsDirectory)
                {
                    try
                    {
                        Directory.Delete(item.FullName, true);
                    }
                    catch
                    {

                    }
                }
                else
                {
                    try
                    {
                        File.Delete(item.FullName);
                    }
                    catch
                    {

                    }
                }
            }

            Refresh();

            InOperation = IsEditMode = false;
        }

        /// <summary>
        /// 全选 / 全不选
        /// </summary>
        public void SelectOrUnselectAll()
        {
            if (!IsEditMode || InOperation) return;

            var allChecked = PathInfos.All(x => x.Checked);
            var setValue = !allChecked;
            foreach (var item in PathInfos)
            {
                item.Checked = setValue;
            }
        }

        /// <summary>
        /// 将选中内容复制到某个目录
        /// </summary>
        public async void CopyToPaths()
        {
            if (!IsSupportFileDelete || InOperation) return;

            var items = PathInfos.Where(x => x.Checked).ToArray();
            var isSingleFile = items.Length == 1 && !items[0].IsDirectory;

            var result = await FilePicker2.SaveAsync(new()
            {
                InitialFileName = isSingleFile ?
                    Path.GetFileName(items[0].FullName) :
                    $"{_ThisAssembly.AssemblyTrademark}  Explorer Export {DateTime.Now.ToString(DateTimeFormat.File)}{FileEx.TAR_GZ}",
            });
            if (result != null)
            {
                InOperation = true;

                using var destStream = result.OpenWrite();
                if (isSingleFile)
                {
                    using var sourceStream = File.OpenRead(items[0].FullName);
                    await sourceStream.CopyToAsync(destStream);
                }
                else
                {
                    using var s = new GZipOutputStream(destStream);
                    s.SetLevel(Deflater.BEST_SPEED);
                    using var archive = TarArchive.CreateOutputTarArchive(s,
                        TarBuffer.DefaultBlockFactor, EncodingCache.UTF8NoBOM);
                    foreach (var item in items)
                    {
                        static void WriteEntry(TarArchive archive, string relativeTo, string path, bool isDirectory)
                        {
                            if (isDirectory)
                            {
                                var files = Directory.GetFiles(path);
                                foreach (var file in files)
                                {
                                    WriteEntry(archive, relativeTo, file, false);
                                }

                                var dirs = Directory.GetDirectories(path);
                                foreach (var dir in dirs)
                                {
                                    WriteEntry(archive, relativeTo, dir, true);
                                }
                            }
                            else
                            {
                                var entry = TarEntry.CreateEntryFromFile(path);
                                entry.Name = Path.GetRelativePath(relativeTo, path);
                                if (Path.DirectorySeparatorChar != IOPath.UnixDirectorySeparatorChar)
                                    entry.Name = entry.Name.Replace(Path.DirectorySeparatorChar, IOPath.UnixDirectorySeparatorChar);
                                archive.WriteEntry(entry, false);
                            }
                        }
                        WriteEntry(archive, _CurrentAbsolutePath, item.FullName, item.IsDirectory);
                    }
                }

                foreach (var item in items)
                {
                    item.Checked = false;
                }

                InOperation = IsEditMode = false;
            }
        }
    }
}