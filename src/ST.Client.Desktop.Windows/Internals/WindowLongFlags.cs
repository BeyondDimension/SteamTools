using System;

[Flags]
internal enum WindowLongFlags
{
    GWL_STYLE = -16,
    GWL_EXSTYLE = -20,
    WS_EX_NOACTIVATE = 0x8000000,
}