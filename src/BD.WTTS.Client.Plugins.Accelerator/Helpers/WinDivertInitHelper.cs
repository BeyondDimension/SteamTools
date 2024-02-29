#if WINDOWS
using WinDivertBinRes = BD.WTTS.Properties.WinDivertResource;

namespace Mobius.Helpers;

/// <summary>
/// 初始化 WinDivert 助手类
/// </summary>
static class WinDivertInitHelper
{
    const string libraryName = "WinDivert.dll";

    static readonly string? libraryPath;
    static readonly string? libraryDirPath;
    //static nint libraryIntPtr;

    ///// <summary>
    ///// 配置本机库路径解析
    ///// </summary>
    ///// <param name="libraryName"></param>
    ///// <param name="assembly"></param>
    ///// <param name="searchPath"></param>
    ///// <returns></returns>
    //public static nint DllImportResolver(string libraryName,
    //    Assembly assembly,
    //    DllImportSearchPath? searchPath)
    //{
    //    switch (libraryName)
    //    {
    //        case WinDivertInitHelper.libraryName:
    //            if (libraryIntPtr != default)
    //                return libraryIntPtr;
    //            break;
    //    }
    //    return default;
    //}

    static bool isInitialize = false;

    /// <summary>
    /// 初始化 WinDivert
    /// </summary>
    public static async Task InitializeAsync()
    {
        if (libraryPath != null && libraryDirPath != null)
        {
            bool isWrite;
            var libraryFileInfo = new FileInfo(libraryPath);
            if (libraryFileInfo.Exists)
            {
                if (libraryFileInfo.Length > 20000000L) // 文件不可能大于 20 MB
                {
                    libraryFileInfo.Delete();
                    isWrite = true;
                }
                else
                {
                    var libraryBytes = await File.ReadAllBytesAsync(libraryPath);
                    var libraryHash = SHA256.HashData(libraryBytes);
                    var libraryHashRes = SHA256.HashData(WinDivertBinRes.WinDivert_dll);
                    if (libraryHash.SequenceEqual(libraryHashRes)) // 哈希值相同，不写入
                    {
                        isWrite = false;
                    }
                    else
                    {
                        libraryFileInfo.Delete();
                        isWrite = true;
                    }
                }
            }
            else
            {
                isWrite = true;
            }

            if (isWrite) // 检查文件不存在时写入
            {
                switch (RuntimeInformation.ProcessArchitecture)
                {
                    //case Architecture.X86:
                    //    break;
                    case Architecture.X64:
                        IOPath.DirCreateByNotExists(libraryDirPath);
                        var task1 = File.WriteAllBytesAsync(Path.Combine(libraryDirPath, "WinDivert64.sys"), WinDivertBinRes.WinDivert64_sys);
                        var task2 = File.WriteAllBytesAsync(libraryPath, WinDivertBinRes.WinDivert_dll);
                        await Task.WhenAll(task1, task2);
                        break;
                }
            }
        }

        if (isInitialize)
            return;
        isInitialize = true;
        //if (IsSupported && libraryPath != null)
        //{
        //    try
        //    {
        //        libraryIntPtr = NativeLibrary.Load(libraryPath);
        //    }
        //    catch
        //    {
        //    }
        //    if (libraryIntPtr != default)
        //    {
        //        NativeLibrary.SetDllImportResolver(typeof(WinDivertSharp.WinDivert).Assembly, DllImportResolver);
        //        return;
        //    }
        //}
        IsSupported = false;
    }

    /// <summary>
    /// 当前运行环境是否支持使用 WinDivert
    /// </summary>
    public static bool IsSupported { get; private set; }

    static WinDivertInitHelper()
    {
        switch (RuntimeInformation.OSArchitecture)
        {
            //case Architecture.X86:
            case Architecture.X64:
                IsSupported = true;
                libraryDirPath = Path.Combine(AppContext.BaseDirectory, "native", "win-x64");
                libraryPath = Path.Combine(libraryDirPath, "WinDivert.dll");
                break;
        }
    }
}
#endif