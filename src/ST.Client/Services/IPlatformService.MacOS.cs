using System;
using System.Collections.Generic;
using System.Text;
#if NETSTANDARD
using JustArchiNET.Madness;
#else
using System.Runtime.Versioning;
#endif

namespace System.Application.Services
{
    partial interface IPlatformService
    {
        [SupportedOSPlatform("MacOS")]
        string[] GetMacNetworkSetup() => throw new PlatformNotSupportedException();
    }
}
