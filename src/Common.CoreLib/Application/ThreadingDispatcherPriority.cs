// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.
// https://github.com/dotnet/wpf/blob/v6.0.5/src/Microsoft.DotNet.Wpf/src/WindowsBase/System/Windows/Threading/DispatcherPriority.cs
// https://github.com/AvaloniaUI/Avalonia/blob/0.10.14/src/Avalonia.Base/Threading/DispatcherPriority.cs

namespace System.Application;

/// <summary>
///     An enunmeration describing the priorities at which
///     operations can be invoked via the Dispatcher.
///     <see cref="https://github.com/dotnet/wpf/blob/master/src/Microsoft.DotNet.Wpf/src/WindowsBase/System/Windows/Threading/DispatcherPriority.cs"/>
/// </summary>
///
public enum ThreadingDispatcherPriority
{
    /// <summary>
    ///     This is an invalid priority.
    /// </summary>
    Invalid = -1,

    /// <summary>
    ///     Operations at this priority are not processed.
    /// </summary>
    Inactive = 0,

    /// <summary>
    ///     Operations at this priority are processed when the system
    ///     is idle.
    /// </summary>
    SystemIdle = 1,

    /// <summary>
    /// Minimum possible priority
    /// </summary>
    MinValue = 1,

    /// <summary>
    ///     Operations at this priority are processed when the application
    ///     is idle.
    /// </summary>
    ApplicationIdle,

    /// <summary>
    ///     Operations at this priority are processed when the context
    ///     is idle.
    /// </summary>
    ContextIdle,

    /// <summary>
    ///     Operations at this priority are processed after all other
    ///     non-idle operations are done.
    /// </summary>
    Background,

    /// <summary>
    ///     Operations at this priority are processed at the same
    ///     priority as input.
    /// </summary>
    Input,

    /// <summary>
    ///     Operations at this priority are processed when layout and render is
    ///     done but just before items at input priority are serviced. Specifically
    ///     this is used while firing the Loaded event
    /// </summary>
    Loaded,

    /// <summary>
    ///     Operations at this priority are processed at the same
    ///     priority as rendering.
    /// </summary>
    Render,

    /// <summary>
    ///     Operations at this priority are processed at the same
    ///     priority as data binding.
    /// </summary>
    DataBind,

    /// <summary>
    ///     Operations at this priority are processed at normal priority.
    /// </summary>
    Normal,

    /// <summary>
    ///     Operations at this priority are processed before other
    ///     asynchronous operations.
    /// </summary>
    Send,

    /// <summary>
    /// Maximum possible priority
    /// </summary>
    MaxValue = Send,
}