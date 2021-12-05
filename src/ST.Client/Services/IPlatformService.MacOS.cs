using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.Versioning;

namespace System.Application.Services
{
    partial interface IPlatformService
    {
        [SupportedOSPlatform("MacOS")]
        string[] GetMacNetworkSetup()
        {
            return Array.Empty<string>();
        }
    }
}
