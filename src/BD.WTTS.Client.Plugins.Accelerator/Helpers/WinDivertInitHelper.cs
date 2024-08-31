#if WINDOWS && !REMOVE_DNS_INTERCEPT
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
        (byte[] WinDivert_dll, byte[] WinDivert64_sys) data = default;

        void GetData()
        {
            if (data == default)
            {
                data = GetWinDivertBinRes();
            }
        }

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
                    GetData();
                    var libraryBytes = await File.ReadAllBytesAsync(libraryPath);
                    var libraryHash = SHA256.HashData(libraryBytes);
                    var libraryHashRes = SHA256.HashData(data.WinDivert_dll!);
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
                GetData();
                switch (RuntimeInformation.ProcessArchitecture)
                {
                    //case Architecture.X86:
                    //    break;
                    case Architecture.X64:
                        IOPath.DirCreateByNotExists(libraryDirPath);
                        var task1 = File.WriteAllBytesAsync(Path.Combine(libraryDirPath, "WinDivert64.sys"), data.WinDivert64_sys!);
                        var task2 = File.WriteAllBytesAsync(libraryPath, data.WinDivert_dll!);
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
                libraryPath = GlobalDllImportResolver.GetLibraryPath("WinDivert.dll");
                libraryDirPath = Path.GetDirectoryName(libraryPath);
                break;
        }
    }

    static (byte[] WinDivert_dll, byte[] WinDivert64_sys) GetWinDivertBinRes()
    {
        byte[] bytes = WinDivertBinRes.WinDivert_mpo;
        bytes.AsSpan().Reverse();

        var data = MemoryPackSerializer.Deserialize<byte[][]>(bytes)!;

        return (data[0], data[1]);
    }
}
#endif