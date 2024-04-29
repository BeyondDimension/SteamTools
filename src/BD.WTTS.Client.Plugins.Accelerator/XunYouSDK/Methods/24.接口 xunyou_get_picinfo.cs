namespace Mobius;

partial class XunYouSDK // 24.接口 xunyou_get_picinfo
{
    /// <summary>
    /// 同步获取对应游戏 id 的图片信息
    /// </summary>
    /// <param name="game_id">迅游对应的游戏 id</param>
    /// <returns></returns>
    public static XunYouPicInfo? GetPicInfo(int game_id)
    {
        try
        {
            unsafe
            {
                int picinfo_size = 0;
                var result = xunyou_get_picinfo(game_id, default, ref picinfo_size);
                if (result == 0)
                {
                    Span<byte> span = new byte[picinfo_size];
                    fixed (byte* span_intptr = span)
                    {
                        result = xunyou_get_picinfo(game_id, span_intptr, ref picinfo_size);
                        if (result == 0) // 获取成功
                        {
                            var jObj = SystemTextJsonSerializer.Deserialize(span,
                                SystemTextJsonSerializerContext_XunYouSDK.Default.XunYouPicInfo);
                            return jObj;
                        }
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Log.Error(nameof(XunYouSDK), ex, "GetPicInfo fail.");
        }

        return null;
    }

    /// <summary>
    /// 同步获取对应游戏 id 的图片信息
    /// </summary>
    /// <param name="game_id">迅游对应的游戏 id</param>
    /// <param name="picinfo">游戏 id 对应的游戏信息，为空则 picinfo_size 会返回所需的空间大小</param>
    /// <param name="picinfo_size">会返回数据的大小</param>
    /// <returns></returns>
    [LibraryImport(libraryName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvStdcall)])]
    private static unsafe partial int xunyou_get_picinfo(int game_id, byte* picinfo,
        ref int picinfo_size);
}