// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.
// https://github.com/dotnet/corefx/blob/master/src/Common/src/CoreLib/System/IO/EncodingCache.cs

using System.Text;

namespace System.IO
{
    public static class EncodingCache
    {
        public static readonly Encoding UTF8NoBOM = new UTF8Encoding(encoderShouldEmitUTF8Identifier: false, throwOnInvalidBytes: true);
    }
}