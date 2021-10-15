using System;
using System.Collections.Generic;
using System.Text;

namespace System.Application.Models
{
    public enum SteamAppPropertyType
    {
        _Invalid_ = -1,
        Table,
        String,
        Int32,
        Float,
        WString = 5,
        Color,
        Uint64,
        _EndOfTable_
    }
}
