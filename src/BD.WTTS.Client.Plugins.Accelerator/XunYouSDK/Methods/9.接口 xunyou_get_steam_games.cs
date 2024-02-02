namespace Mobius;

partial class XunYouSDK // 9.接口 xunyou_get_steam_games
{
    /// <summary>
    /// 获取可加速的 steam 游戏列表。
    /// </summary>
    /// <returns></returns>
    public static XunYouSteamGame[]? GetSteamGames()
    {
        try
        {
            unsafe
            {
                int gamelist_size = 0;
                var result = xunyou_get_steam_games(default, ref gamelist_size);
                if (result == 0)
                {
                    Span<byte> gamelist = new byte[gamelist_size];
                    fixed (byte* gamelist_intptr = gamelist)
                    {
                        result = xunyou_get_steam_games(gamelist_intptr, ref gamelist_size);
                        if (result == 0) // 获取成功
                        {
                            //return Encoding.UTF8.GetString(gamelist);
                            var jObj = SystemTextJsonSerializer.Deserialize(gamelist,
                                SystemTextJsonSerializerContext_XunYouSDK.Default.xunyou_get_gamelist_resultXunYouSteamGame);
                            return jObj?.Data?.GameList;
                        }
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Log.Error(nameof(XunYouSDK), "GetSteamGames fail.", ex);
        }

        return null;
    }

    /// <summary>
    /// 获取可加速的 steam 游戏列表。
    /// </summary>
    /// <param name="gamelist">支持加速的 steam 游戏列表，为空则 gamelist_size 会返回所需的空间大小。</param>
    /// <param name="gamelist_size">sizeof(gamelist)</param>
    /// <returns></returns>
    [LibraryImport(libraryName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvStdcall)])]
    private static unsafe partial int xunyou_get_steam_games(byte* gamelist,
        ref int gamelist_size);
}