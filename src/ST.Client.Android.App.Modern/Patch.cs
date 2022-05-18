//using System;
//using System.Globalization;
//using System.Reflection;
//using HarmonyLib;

//namespace Microsoft.Net.Http.Headers;

//static class DateTimeFormatterPatch
//{
//    public static void Init()
//    {
//        // .nuget\packages\microsoft.net.http.headers\2.2.8\lib\netstandard2.0\Microsoft.Net.Http.Headers.dll
//        var classType = typeof(HeaderUtilities).Assembly.GetType("Microsoft.Net.Http.Headers.DateTimeFormatter");
//        if (classType == null) return;
//        var method = classType.GetMethod("ToRfc1123String", BindingFlags.Static | BindingFlags.Public, new[]
//        {
//            typeof(DateTimeOffset),
//            typeof(bool),
//        });
//        if (method == null) return;
//        var h = new Harmony(nameof(DateTimeFormatterPatch));
//        try
//        {
//            h.Patch(method, new HarmonyMethod(typeof(DateTimeFormatterPatch).GetMethod(nameof(Prefix), BindingFlags.Static | BindingFlags.NonPublic)));
//        }
//        catch (Exception ex)
//        {

//        }
//    }

//#pragma warning disable SA1313 // Parameter names should begin with lower-case letter
//    static bool Prefix(ref string __result, DateTimeOffset dateTime, bool quoted)
//#pragma warning restore SA1313 // Parameter names should begin with lower-case letter
//    {
//        __result = FormatDate(dateTime, quoted);
//        return true; // make sure you only skip if really necessary
//    }
//    // https://github.com/dotnet/aspnetcore/blob/v2.2.13/src/Http/Headers/src/DateTimeFormatter.cs#L40

//    /// <summary>
//    /// Formats the <paramref name="dateTime"/> using the RFC1123 format specifier and optionally quotes it.
//    /// </summary>
//    /// <param name="dateTime">The date to format.</param>
//    /// <param name="quoted">Determines if the formatted date should be quoted.</param>
//    /// <returns>The formatted date.</returns>
//    static string FormatDate(DateTimeOffset dateTime, bool quoted)
//    {
//        // https://github.com/dotnet/aspnetcore/blob/v6.0.5/src/Http/Headers/src/HeaderUtilities.cs#L548
//        if (quoted)
//        {
//            return string.Create(31, dateTime, (span, dt) =>
//            {
//                span[0] = span[30] = '"';
//                dt.TryFormat(span.Slice(1), out _, "r");
//            });
//        }

//        return dateTime.ToString("r", CultureInfo.InvariantCulture);
//    }
//}