#pragma warning disable IDE0079 // 请删除不必要的忽略
#pragma warning disable IDE0130 // 命名空间与文件夹结构不匹配
#pragma warning restore IDE0079 // 请删除不必要的忽略
namespace Mobius;

static partial class XunYouSDK
{
#if WINDOWS
    static readonly string? libraryPath;
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
    //    if (appId != 0)
    //    {
    //        switch (libraryName)
    //        {
    //            case XunYouSDK.libraryName:
    //                if (libraryIntPtr != default)
    //                    return libraryIntPtr;
    //                break;
    //        }
    //    }
    //    return default;
    //}

    //static bool isInitialize = false;

    /// <summary>
    /// 初始化 <see cref="XunYouSDK"/>
    /// </summary>
    public static void Initialize()
    {
        //if (isInitialize)
        //    return;
        //isInitialize = true;
        ////if (IsSupported && libraryPath != null)
        ////{
        ////    //try
        ////    //{
        ////    //    libraryIntPtr = NativeLibrary.Load(libraryPath);
        ////    //}
        ////    //catch
        ////    //{
        ////    //}
        ////    //if (libraryIntPtr != default)
        ////    //{
        ////    //    //NativeLibrary.SetDllImportResolver(typeof(XunYouSDK).Assembly, DllImportResolver);
        ////    //    return;
        ////    //}
        ////}
        ////IsSupported = false;
    }
#endif

    /// <summary>
    /// 当前运行环境是否支持使用迅游加速器
    /// </summary>
    public static bool IsSupported { get; private set; }

#if WINDOWS
    static XunYouSDK()
    {
        if (appId != 0)
        {
            switch (RuntimeInformation.OSArchitecture)
            {
                case Architecture.X86:
                case Architecture.X64:
                    IsSupported = true;
                    switch (RuntimeInformation.ProcessArchitecture)
                    {
                        case Architecture.X86:
                            libraryPath = Path.GetFullPath(Path.Combine(typeof(XunYouSDK).Assembly.Location, "..", libraryFileNameX86));
                            break;
                        case Architecture.X64:
                            libraryPath = Path.GetFullPath(Path.Combine(typeof(XunYouSDK).Assembly.Location, "..", libraryFileNameX64));
                            break;
                    }
                    break;
            }
        }
    }
#endif
}
