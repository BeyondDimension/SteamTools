// Copyright (c) .NET Foundation and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.
// https://github.com/dotnet/cli-lab/blob/1.5.255402/src/dotnet-core-uninstall/Shared/Exceptions/InvalidInputVersionException.cs

namespace Microsoft.DotNet.Tools.Uninstall.Shared.Exceptions
{
    internal class InvalidInputVersionException : DotNetUninstallException
    {
        public InvalidInputVersionException(string versionString) :
            base(string.Format(LocalizableStrings.InvalidInputVersionExceptionMessageFormat, versionString))
        { }
    }
}