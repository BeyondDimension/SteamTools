namespace Mobius;

partial class XunYouSDK // 15.接口 xunyou_get_gameinfo
{
    //static XunYouGameInfo? SortGameInfo(XunYouGameInfo? gameInfo)
    //{
    //    if (gameInfo?.Areas != null)
    //    {
    //        foreach (var item in gameInfo.Areas)
    //        {
    //            if (item.Servers == null || item.Servers.Count == 0)
    //                continue;
    //            item.Servers = item.Servers.OrderBy(x => x.Id).ToList();
    //        }

    //        gameInfo.Areas = gameInfo.Areas.OrderBy(x => x.Id).ToList();
    //    }

    //    return gameInfo;
    //}

    /// <summary>
    /// 获取对应游戏 Id 的游戏信息（平台信息，区服信息）
    /// </summary>
    /// <param name="gameId">迅游对应的游戏 Id</param>
    /// <returns></returns>
    public static XunYouGameInfo? GetGameInfo(int gameId)
    {
        try
        {
            unsafe
            {
                int gamelist_size = 0;
                var result = xunyou_get_gameinfo(gameId, default, ref gamelist_size);
                if (result == 0)
                {
                    Span<byte> gamelist = new byte[gamelist_size];
                    fixed (byte* gamelist_intptr = gamelist)
                    {
                        result = xunyou_get_gameinfo(gameId, gamelist_intptr, ref gamelist_size);
                        if (result == 0) // 获取成功
                        {
                            //return Encoding.UTF8.GetString(gamelist);
                            var jObj = SystemTextJsonSerializer.Deserialize(gamelist,
                                SystemTextJsonSerializerContext_XunYouSDK.Default.xunyou_get_gameinfo_resultXunYouGameInfo);
                            //return SortGameInfo(jObj?.GameInfo);
                            return jObj?.GameInfo; // 由 SDK 返回排序后的数据
                        }
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Log.Error(nameof(XunYouSDK), "GetHotGames fail.", ex);
        }

        return null;
    }

    /// <summary>
    /// 获取对应游戏 Id 的游戏信息（平台信息，区服信息）
    /// </summary>
    /// <param name="game_id">迅游对应的游戏 Id/param>
    /// <param name="gamelist">游戏 Id 对应的游戏信息，为空则 gameinfo_size 会返回所需的空间大小，该指针的生命周期由调用方维护。</param>
    /// <param name="gamelist_size">sizeof(gamelist)</param>
    /// <returns></returns>
    [LibraryImport(libraryName)]
    [UnmanagedCallConv(CallConvs = [typeof(CallConvStdcall)])]
    private static unsafe partial int xunyou_get_gameinfo(int game_id, byte* gameinfo,
        ref int gameinfo_size);
}