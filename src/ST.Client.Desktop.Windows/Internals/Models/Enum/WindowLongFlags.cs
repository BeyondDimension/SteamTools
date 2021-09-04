using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace System
{
    [Flags]
    public enum WindowLongFlags
    {
        GWL_STYLE = -16,
        GWL_EXSTYLE = -20,
        WS_EX_NOACTIVATE = 0x8000000,
    }
}
