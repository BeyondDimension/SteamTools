using dotnetCampus.Ipc.CompilerServices.Attributes;

// ReSharper disable once CheckNamespace
namespace BD.WTTS.Services;

/// <summary>
/// 子进程模块的 IPC 服务，调用 <see cref="IDisposable.Dispose"/> 退出子进程
/// </summary>
[IpcPublic(Timeout = AssemblyInfo.IpcTimeout, IgnoresIpcException = false)]
public interface IPCSubProcessModuleService : IDisposable
{
    internal static class Constants
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string GetClientPipeName(string moduleName, string pipeName)
            => $"{pipeName}_{moduleName}";

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string GetDirectoryName(string name)
        {
            var chars = GetDirectoryNameCore(name).ToArray();
            if (chars.Length > 0) return new string(chars);
            return $"{name.Length}_{Hashs.String.SHA256(name)}";

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            static IEnumerable<char> GetDirectoryNameCore(string name)
            {
                var chars = Path.GetInvalidFileNameChars();
                foreach (var item in name)
                {
                    if (!chars.Contains(item))
                    {
                        yield return item;
                    }
                }
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static string GetPluginsDirectory(string pluginName, string directory)
        {
            var dirName = GetDirectoryName(pluginName);
            directory = Path.Combine(directory, AssemblyInfo.Plugins, dirName);
            IOPath.DirCreateByNotExists(directory);
            return directory;
        }
    }
}
