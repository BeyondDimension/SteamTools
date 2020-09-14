using System;
using System.Runtime.InteropServices;

namespace Steam4NET
{
    [StructLayout(LayoutKind.Explicit, Size = 8)]
    public struct GameID_t
    {
        [FieldOffset(0)]
        // /!\ C# doesn't support bit fields, use this field like this: m_nAppID & 0xFFFFFF
        public UInt32 m_nAppID; // : 24
        [FieldOffset(3)]
        public UInt32 m_nType; // : 8
        [FieldOffset(4)]
        public UInt32 m_nModID; // : 32
    }
}
