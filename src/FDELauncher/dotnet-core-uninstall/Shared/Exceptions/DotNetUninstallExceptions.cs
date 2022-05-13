// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// https://github.com/dotnet/cli-lab/blob/1.5.255402/src/dotnet-core-uninstall/Shared/Exceptions/DotNetUninstallExceptions.cs

using System;

namespace Microsoft.DotNet.Tools.Uninstall.Shared.Exceptions
{
    internal abstract class DotNetUninstallException : Exception
    {
        public DotNetUninstallException(string message) : base(message) { }
    }
}