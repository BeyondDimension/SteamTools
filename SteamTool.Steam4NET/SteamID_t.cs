using System;
using System.Runtime.InteropServices;

namespace Steam4NET
{
    // Summary:
    //     Used to store a SteamID in callbacks (With proper alignment / padding).
    //     You probably don't want to use this type directly, convert it to CSteamID.
    [StructLayout(LayoutKind.Sequential, Pack= 8)]
    public struct SteamID_t
    {
        public UInt32 low32Bits;    // m_unAccountID (32)
        public UInt32 high32Bits;   // m_unAccountInstance (20), m_EAccountType (4), m_EUniverse (8)
    }
}
