// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// https://github.com/dotnet/cli-lab/blob/1.5.255402/src/dotnet-core-uninstall/Shared/BundleInfo/BundleType.cs

using System;

namespace Microsoft.DotNet.Tools.Uninstall.Shared.BundleInfo
{
    [Flags]
    internal enum BundleType
    {
        Sdk = 0x1,
        Runtime = 0x2,
        AspNetRuntime = 0x4,
        HostingBundle = 0x8
    }
}