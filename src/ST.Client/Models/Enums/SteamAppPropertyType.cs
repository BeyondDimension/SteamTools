using System;
using System.Collections.Generic;
using System.Text;

namespace System.Application.Models
{
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
        _EndOfTable_ = 8
    }
}
