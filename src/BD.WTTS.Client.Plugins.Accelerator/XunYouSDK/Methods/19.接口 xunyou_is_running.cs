namespace Mobius;

partial class XunYouSDK // 19.接口 xunyou_is_running
{
    /// <summary>
    /// 获取当前加速器是否在运行。
    /// </summary>
    /// <param name="id">合作id，由迅游给出明确值。</param>
    /// <returns>0 已启动, -1 未启动</returns>
    [LibraryImport(libraryName, EntryPoint = "xunyou_is_running")]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvStdcall)])]
    public static partial XunYouIsRunningCode IsRunning(int id = appId);
}