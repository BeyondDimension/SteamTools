#pragma warning disable IDE0079 // 请删除不必要的忽略
#pragma warning disable SA1413 // Use trailing comma in multi-line initializers

namespace BD.WTTS.Enums;

public enum SteamAppPropertyType
{
    _Invalid_ = -1,
    Table = 0,
    String = 1,
    Int32 = 2,
    Float = 3,
    WString = 5,
    Color = 6,
    Uint64 = 7,
    _EndOfTable_ = 8,
}
