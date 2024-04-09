namespace Mobius;

partial class XunYouSDK // 23.接口 xunyou_start_game
{
    /// <summary>
    /// 调用加速器默认启动当前加速的游戏
    /// </summary>
    /// <returns></returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static XunYouStartGameCode StartGame()
    {
        int result = xunyou_start_game();
        return (XunYouStartGameCode)result;
    }

    [LibraryImport(libraryName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvStdcall)])]
    private static unsafe partial int xunyou_start_game();
}