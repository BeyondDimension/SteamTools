// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.
// https://github.com/dotnet/wpf/blob/v6.0.6/src/Microsoft.DotNet.Wpf/src/PresentationCore/SR.cs

#if !NETFRAMEWORK && WINDOWS

namespace MS.Internal.PresentationCore;

internal static class SR
{
    public static string Get(string name) => name switch
    {
        SRID.CantShowMBServiceWithOwner => "Cannot show MessageBox Service with Owner.",
        _ => "",
    };
}

#endif