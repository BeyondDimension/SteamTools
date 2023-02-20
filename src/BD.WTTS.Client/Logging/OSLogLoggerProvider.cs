#if MACOS || MACCATALYST || IOS
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.
// https://github.com/unoplatform/uno.extensions.logging/blob/1.4.0/src/Uno.Extensions.Logging.OSLog/OSLogLoggerProvider.cs

// ReSharper disable once CheckNamespace
namespace BD.WTTS.Logging;

/// <summary>
/// A provider of <see cref="OSLogLogger"/> instances.
/// </summary>
public sealed class OSLogLoggerProvider : ILoggerProvider
{
    readonly ConcurrentDictionary<string, OSLogLogger> _loggers;

    /// <summary>
    /// Creates an instance of <see cref="OSLogLoggerProvider"/>.
    /// </summary>
    OSLogLoggerProvider()
    {
        _loggers = new ConcurrentDictionary<string, OSLogLogger>();
    }

    /// <inheritdoc />
    public ILogger CreateLogger(string name)
    {
        return _loggers.GetOrAdd(name, loggerName => new OSLogLogger(name));
    }

    /// <inheritdoc />
    public void Dispose()
    {
    }

    public static ILoggerProvider Instance { get; } = new OSLogLoggerProvider();
}
#endif