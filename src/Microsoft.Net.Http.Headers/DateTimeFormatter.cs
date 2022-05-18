// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Globalization;

namespace Microsoft.Net.Http.Headers
{
    internal static class DateTimeFormatter
    {
        public static string ToRfc1123String(this DateTimeOffset dateTime)
        {
            return ToRfc1123String(dateTime, false);
        }

        public static string ToRfc1123String(this DateTimeOffset dateTime, bool quoted)
        {
            // https://github.com/dotnet/aspnetcore/blob/v6.0.5/src/Http/Headers/src/HeaderUtilities.cs#L548
            if (quoted)
            {
                return string.Create(31, dateTime, (span, dt) =>
                {
                    span[0] = span[30] = '"';
                    dt.TryFormat(span.Slice(1), out _, "r");
                });
            }

            return dateTime.ToString("r", CultureInfo.InvariantCulture);
        }
    }
}
