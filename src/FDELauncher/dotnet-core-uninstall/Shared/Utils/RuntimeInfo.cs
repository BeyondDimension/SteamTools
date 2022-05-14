// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// https://github.com/dotnet/cli-lab/blob/1.5.255402/src/dotnet-core-uninstall/Shared/Utils/RuntimeInfo.cs

using System;

namespace Microsoft.DotNet.Tools.Uninstall.Shared.Utils
{
    internal static class RuntimeInfo
    {
        static RuntimeInfo()
        {
            switch (Environment.OSVersion.Platform)
            {
                case PlatformID.Win32NT:
                    RunningOnWindows = true;
                    break;
                case PlatformID.Unix:
                    RunningOnLinux = true;
                    break;
                case PlatformID.MacOSX:
                    RunningOnOSX = true;
                    break;
            }
        }

        public static readonly bool RunningOnWindows;
        public static readonly bool RunningOnOSX;
        public static readonly bool RunningOnLinux;
    }
}