// 
// Copyright (c) 2004-2021 Jaroslaw Kowalski <jaak@jkowalski.net>, Kim Christensen, Julian Verdurmen
// 
// All rights reserved.
// 
// Redistribution and use in source and binary forms, with or without 
// modification, are permitted provided that the following conditions 
// are met:
// 
// * Redistributions of source code must retain the above copyright notice, 
//   this list of conditions and the following disclaimer. 
// 
// * Redistributions in binary form must reproduce the above copyright notice,
//   this list of conditions and the following disclaimer in the documentation
//   and/or other materials provided with the distribution. 
// 
// * Neither the name of Jaroslaw Kowalski nor the names of its 
//   contributors may be used to endorse or promote products derived from this
//   software without specific prior written permission. 
// 
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS"
// AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE 
// IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE 
// ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT OWNER OR CONTRIBUTORS BE 
// LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR 
// CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF
// SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS 
// INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN 
// CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) 
// ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF 
// THE POSSIBILITY OF SUCH DAMAGE.
// https://github.com/NLog/NLog/blob/v5.0/src/NLog/LogManager.cs

using JetBrains.Annotations;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using NLog;
using NLog.Common;
using NLog.Config;
using NLog.Internal;

namespace ArchiSteamFarm
{
    /// <summary>
    /// Creates and manages instances of <see cref="Logger" /> objects.
    /// </summary>
    /// <remarks>
    /// LogManager wraps a singleton instance of <see cref="global::NLog.LogFactory" />.
    /// </remarks>
    public static class LogManager
    {
        /// <remarks>
        /// Internal for unit tests
        /// </remarks>
        internal static readonly LogFactory factory = new();
        private static ICollection<Assembly>? _hiddenAssemblies;

        private static readonly object lockObject = new();

        /// <summary>
        /// Gets the <see cref="global::NLog.LogFactory" /> instance used in the <see cref="LogManager"/>.
        /// </summary>
        /// <remarks>Could be used to pass the to other methods</remarks>
        public static LogFactory LogFactory => factory;

        /// <summary>
        /// Occurs when logging <see cref="Configuration" /> changes.
        /// </summary>
        public static event EventHandler<LoggingConfigurationChangedEventArgs> ConfigurationChanged
        {
            add => factory.ConfigurationChanged += value;
            remove => factory.ConfigurationChanged -= value;
        }

#if !NETSTANDARD1_3
        /// <summary>
        /// Occurs when logging <see cref="Configuration" /> gets reloaded.
        /// </summary>
        public static event EventHandler<LoggingConfigurationReloadedEventArgs> ConfigurationReloaded
        {
            add => factory.ConfigurationReloaded += value;
            remove => factory.ConfigurationReloaded -= value;
        }
#endif
        /// <summary>
        /// Gets or sets a value indicating whether NLog should throw exceptions. 
        /// By default exceptions are not thrown under any circumstances.
        /// </summary>
        public static bool ThrowExceptions
        {
            get => factory.ThrowExceptions;
            set => factory.ThrowExceptions = value;
        }

        /// <summary>
        /// Gets or sets a value indicating whether <see cref="NLogConfigurationException"/> should be thrown.
        /// </summary>
        /// <value>A value of <c>true</c> if exception should be thrown; otherwise, <c>false</c>.</value>
        /// <remarks>
        /// This option is for backwards-compatibility.
        /// By default exceptions are not thrown under any circumstances.
        /// 
        /// </remarks>
        public static bool? ThrowConfigExceptions
        {
            get => factory.ThrowConfigExceptions;
            set => factory.ThrowConfigExceptions = value;
        }

        /// <summary>
        /// Gets or sets a value indicating whether Variables should be kept on configuration reload.
        /// </summary>
        public static bool KeepVariablesOnReload
        {
            get => factory.KeepVariablesOnReload;
            set => factory.KeepVariablesOnReload = value;
        }

        /// <summary>
        /// Gets or sets a value indicating whether to automatically call <see cref="Shutdown"/>
        /// on AppDomain.Unload or AppDomain.ProcessExit
        /// </summary>
        public static bool AutoShutdown
        {
            get => factory.AutoShutdown;
            set => factory.AutoShutdown = value;
        }

        /// <summary>
        /// Gets or sets the current logging configuration.
        /// </summary>
        /// <remarks>
        /// Setter will re-configure all <see cref="Logger"/>-objects, so no need to also call <see cref="ReconfigExistingLoggers()" />
        /// </remarks>
        public static LoggingConfiguration Configuration
        {
            get => factory.Configuration;
            set => factory.Configuration = value;
        }

        /// <summary>
        /// Begins configuration of the LogFactory options using fluent interface
        /// </summary>
        public static ISetupBuilder Setup()
        {
            return LogFactory.Setup();
        }

        /// <summary>
        /// Begins configuration of the LogFactory options using fluent interface
        /// </summary>
        public static LogFactory Setup(Action<ISetupBuilder> setupBuilder)
        {
            return LogFactory.Setup(setupBuilder);
        }

        /// <summary>
        /// Loads logging configuration from file (Currently only XML configuration files supported)
        /// </summary>
        /// <param name="configFile">Configuration file to be read</param>
        /// <returns>LogFactory instance for fluent interface</returns>
        public static LogFactory LoadConfiguration(string configFile)
        {
            factory.LoadConfiguration(configFile);
            return factory;
        }

        /// <summary>
        /// Gets or sets the global log threshold. Log events below this threshold are not logged.
        /// </summary>
        public static LogLevel GlobalThreshold
        {
            get => factory.GlobalThreshold;
            set => factory.GlobalThreshold = value;
        }

        /// <summary>
        /// Gets the logger with the full name of the current class, so namespace and class name.
        /// </summary>
        /// <returns>The logger.</returns>
        /// <remarks>This is a slow-running method. 
        /// Make sure you're not doing this in a loop.</remarks>
        [CLSCompliant(false)]
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static Logger GetCurrentClassLogger()
        {
            return factory.GetLogger(StackTraceUsageUtils.GetClassFullName());
        }

        internal static bool IsHiddenAssembly(Assembly assembly)
        {
            return _hiddenAssemblies != null && _hiddenAssemblies.Contains(assembly);
        }

        /// <summary>
        /// Adds the given assembly which will be skipped 
        /// when NLog is trying to find the calling method on stack trace.
        /// </summary>
        /// <param name="assembly">The assembly to skip.</param>
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static void AddHiddenAssembly(Assembly assembly)
        {
            lock (lockObject)
            {
                if (_hiddenAssemblies != null && _hiddenAssemblies.Contains(assembly))
                    return;

                _hiddenAssemblies = new HashSet<Assembly>(_hiddenAssemblies ?? Enumerable.Empty<Assembly>())
                {
                    assembly
                };
            }

            InternalLogger.Trace("Assembly '{0}' will be hidden in callsite stacktrace", assembly?.FullName);
        }

        /// <summary>
        /// Gets a custom logger with the full name of the current class, so namespace and class name.
        /// Use <paramref name="loggerType"/> to create instance of a custom <see cref="Logger"/>.
        /// If you haven't defined your own <see cref="Logger"/> class, then use the overload without the loggerType.
        /// </summary>
        /// <param name="loggerType">The logger class. This class must inherit from <see cref="Logger" />.</param>
        /// <returns>The logger of type <paramref name="loggerType"/>.</returns>
        /// <remarks>This is a slow-running method. 
        /// Make sure you're not doing this in a loop.</remarks>
        [CLSCompliant(false)]
        [MethodImpl(MethodImplOptions.NoInlining)]
        public static Logger GetCurrentClassLogger(Type loggerType)
        {
            return factory.GetLogger(StackTraceUsageUtils.GetClassFullName(), loggerType);
        }

        /// <summary>
        /// Creates a logger that discards all log messages.
        /// </summary>
        /// <returns>Null logger which discards all log messages.</returns>
        public static Logger CreateNullLogger()
        {
            return factory.CreateNullLogger();
        }

        /// <summary>
        /// Gets the specified named logger.
        /// </summary>
        /// <param name="name">Name of the logger.</param>
        /// <returns>The logger reference. Multiple calls to <c>GetLogger</c> with the same argument aren't guaranteed to return the same logger reference.</returns>
        public static Logger GetLogger(string name)
        {
            return factory.GetLogger(name);
        }

        /// <summary>
        /// Gets the specified named custom logger.
        /// Use <paramref name="loggerType"/> to create instance of a custom <see cref="Logger"/>.
        /// If you haven't defined your own <see cref="Logger"/> class, then use the overload without the loggerType.
        /// </summary>
        /// <param name="name">Name of the logger.</param>
        /// <param name="loggerType">The logger class. This class must inherit from <see cref="Logger" />.</param>
        /// <returns>The logger of type <paramref name="loggerType"/>. Multiple calls to <c>GetLogger</c> with the same argument aren't guaranteed to return the same logger reference.</returns>
        /// <remarks>The generic way for this method is <see cref="LogFactory{loggerType}.GetLogger(string)"/></remarks>
        public static Logger GetLogger(string name, Type loggerType)
        {
            return factory.GetLogger(name, loggerType);
        }

        /// <summary>
        /// Loops through all loggers previously returned by GetLogger.
        /// and recalculates their target and filter list. Useful after modifying the configuration programmatically
        /// to ensure that all loggers have been properly configured.
        /// </summary>
        public static void ReconfigExistingLoggers()
        {
            factory.ReconfigExistingLoggers();
        }

        /// <summary>
        /// Loops through all loggers previously returned by GetLogger.
        /// and recalculates their target and filter list. Useful after modifying the configuration programmatically
        /// to ensure that all loggers have been properly configured.
        /// </summary>
        /// <param name="purgeObsoleteLoggers">Purge garbage collected logger-items from the cache</param>
        public static void ReconfigExistingLoggers(bool purgeObsoleteLoggers)
        {
            factory.ReconfigExistingLoggers(purgeObsoleteLoggers);
        }

        /// <summary>
        /// Flush any pending log messages (in case of asynchronous targets) with the default timeout of 15 seconds.
        /// </summary>
        public static void Flush()
        {
            factory.Flush();
        }

        /// <summary>
        /// Flush any pending log messages (in case of asynchronous targets).
        /// </summary>
        /// <param name="timeout">Maximum time to allow for the flush. Any messages after that time will be discarded.</param>
        public static void Flush(TimeSpan timeout)
        {
            factory.Flush(timeout);
        }

        /// <summary>
        /// Flush any pending log messages (in case of asynchronous targets).
        /// </summary>
        /// <param name="timeoutMilliseconds">Maximum time to allow for the flush. Any messages after that time will be discarded.</param>
        public static void Flush(int timeoutMilliseconds)
        {
            factory.Flush(timeoutMilliseconds);
        }

        /// <summary>
        /// Flush any pending log messages (in case of asynchronous targets).
        /// </summary>
        /// <param name="asyncContinuation">The asynchronous continuation.</param>
        public static void Flush(AsyncContinuation asyncContinuation)
        {
            factory.Flush(asyncContinuation);
        }

        /// <summary>
        /// Flush any pending log messages (in case of asynchronous targets).
        /// </summary>
        /// <param name="asyncContinuation">The asynchronous continuation.</param>
        /// <param name="timeout">Maximum time to allow for the flush. Any messages after that time will be discarded.</param>
        public static void Flush(AsyncContinuation asyncContinuation, TimeSpan timeout)
        {
            factory.Flush(asyncContinuation, timeout);
        }

        /// <summary>
        /// Flush any pending log messages (in case of asynchronous targets).
        /// </summary>
        /// <param name="asyncContinuation">The asynchronous continuation.</param>
        /// <param name="timeoutMilliseconds">Maximum time to allow for the flush. Any messages after that time will be discarded.</param>
        public static void Flush(AsyncContinuation asyncContinuation, int timeoutMilliseconds)
        {
            factory.Flush(asyncContinuation, timeoutMilliseconds);
        }

        /// <summary>
        /// Suspends the logging, and returns object for using-scope so scope-exit calls <see cref="EnableLogging"/>
        /// </summary>
        /// <remarks>
        /// Logging is suspended when the number of <see cref="DisableLogging"/> calls are greater 
        /// than the number of <see cref="EnableLogging"/> calls.
        /// </remarks>
        /// <returns>An object that implements IDisposable whose Dispose() method re-enables logging. 
        /// To be used with C# <c>using ()</c> statement.</returns>
        [Obsolete("Use SuspendLogging() instead. Marked obsolete on NLog 5.0")]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public static IDisposable DisableLogging()
        {
            return factory.SuspendLogging();
        }

        /// <summary>
        /// Resumes logging if having called <see cref="DisableLogging"/>.
        /// </summary>
        /// <remarks>
        /// Logging is suspended when the number of <see cref="DisableLogging"/> calls are greater 
        /// than the number of <see cref="EnableLogging"/> calls.
        /// </remarks>
        [Obsolete("Use ResumeLogging() instead. Marked obsolete on NLog 5.0")]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public static void EnableLogging()
        {
            factory.ResumeLogging();
        }

        /// <summary>
        /// Suspends the logging, and returns object for using-scope so scope-exit calls <see cref="ResumeLogging"/>
        /// </summary>
        /// <remarks>
        /// Logging is suspended when the number of <see cref="SuspendLogging"/> calls are greater 
        /// than the number of <see cref="ResumeLogging"/> calls.
        /// </remarks>
        /// <returns>An object that implements IDisposable whose Dispose() method re-enables logging. 
        /// To be used with C# <c>using ()</c> statement.</returns>
        public static IDisposable SuspendLogging()
        {
            return factory.SuspendLogging();
        }

        /// <summary>
        /// Resumes logging if having called <see cref="SuspendLogging"/>.
        /// </summary>
        /// <remarks>
        /// Logging is suspended when the number of <see cref="SuspendLogging"/> calls are greater 
        /// than the number of <see cref="ResumeLogging"/> calls.
        /// </remarks>
        public static void ResumeLogging()
        {
            factory.ResumeLogging();
        }

        /// <summary>
        /// Returns <see langword="true" /> if logging is currently enabled.
        /// </summary>
        /// <remarks>
        /// Logging is suspended when the number of <see cref="SuspendLogging"/> calls are greater 
        /// than the number of <see cref="ResumeLogging"/> calls.
        /// </remarks>
        /// <returns>A value of <see langword="true" /> if logging is currently enabled, 
        /// <see langword="false"/> otherwise.</returns>
        public static bool IsLoggingEnabled()
        {
            return factory.IsLoggingEnabled();
        }

        /// <summary>
        /// Dispose all targets, and shutdown logging.
        /// </summary>
        public static void Shutdown()
        {
            factory.Shutdown();
        }

        static class StackTraceUsageUtils
        {
            private static readonly Assembly nlogAssembly = typeof(StackTraceUsageUtils).GetAssembly();
            private static readonly Assembly mscorlibAssembly = typeof(string).GetAssembly();
            private static readonly Assembly systemAssembly = typeof(Debug).GetAssembly();

            /// <summary>
            /// Gets the fully qualified name of the class invoking the calling method, including the 
            /// namespace but not the assembly.    
            /// </summary>
            [MethodImpl(MethodImplOptions.NoInlining)]
            public static string GetClassFullName()
            {
                int framesToSkip = 2;

                string className = string.Empty;
#if !NETSTANDARD1_3 && !NETSTANDARD1_5
                var stackFrame = new StackFrame(framesToSkip, false);
                className = GetClassFullName(stackFrame);
#else
            var stackTrace = Environment.StackTrace;
            var stackTraceLines = stackTrace.Replace("\r", "").SplitAndTrimTokens('\n');
            for (int i = 0; i < stackTraceLines.Length; ++i)
            {
                var callingClassAndMethod = stackTraceLines[i].Split(new[] { " ", "<>", "(", ")" }, StringSplitOptions.RemoveEmptyEntries)[1];
                int methodStartIndex = callingClassAndMethod.LastIndexOf(".", StringComparison.Ordinal);
                if (methodStartIndex > 0)
                {
                    // Trim method name. 
                    var callingClass = callingClassAndMethod.Substring(0, methodStartIndex);
                    // Needed because of extra dot, for example if method was .ctor()
                    className = callingClass.TrimEnd('.');
                    if (!className.StartsWith("System.Environment", StringComparison.Ordinal) && framesToSkip != 0)
                    {
                        i += framesToSkip - 1;
                        framesToSkip = 0;
                        continue;
                    }
                    if (!className.StartsWith("System.", StringComparison.Ordinal))
                        break;
                }
            }
#endif
                return className;
            }

#if !NETSTANDARD1_3 && !NETSTANDARD1_5
            /// <summary>
            /// Gets the fully qualified name of the class invoking the calling method, including the 
            /// namespace but not the assembly.
            /// </summary>
            /// <param name="stackFrame">StackFrame from the calling method</param>
            /// <returns>Fully qualified class name</returns>
            public static string GetClassFullName(StackFrame stackFrame)
            {
                string className = LookupClassNameFromStackFrame(stackFrame);
                if (string.IsNullOrEmpty(className))
                {
                    var stackTrace = new StackTrace(false);
                    className = GetClassFullName(stackTrace);
                    if (string.IsNullOrEmpty(className))
                        className = stackFrame.GetMethod()?.Name ?? string.Empty;
                }
                return className;
            }
#endif

            private static string GetClassFullName(StackTrace stackTrace)
            {
                foreach (StackFrame frame in stackTrace.GetFrames())
                {
                    string className = LookupClassNameFromStackFrame(frame);
                    if (!string.IsNullOrEmpty(className))
                    {
                        return className;
                    }
                }
                return string.Empty;
            }

            /// <summary>
            /// Returns the assembly from the provided StackFrame (If not internal assembly)
            /// </summary>
            /// <returns>Valid assembly, or null if assembly was internal</returns>
            public static Assembly? LookupAssemblyFromStackFrame(StackFrame stackFrame)
            {
                var method = stackFrame.GetMethod();
                if (method is null)
                {
                    return null;
                }

                var assembly = method.DeclaringType?.GetAssembly() ?? method.Module?.Assembly;
                // skip stack frame if the method declaring type assembly is from hidden assemblies list
                if (assembly == nlogAssembly)
                {
                    return null;
                }

                if (assembly == mscorlibAssembly)
                {
                    return null;
                }

                if (assembly == systemAssembly)
                {
                    return null;
                }

                return assembly;
            }


            public static string? GetStackFrameMethodClassName(MethodBase method, bool includeNameSpace, bool cleanAsyncMoveNext, bool cleanAnonymousDelegates)
            {
                if (method is null)
                    return null;

                var callerClassType = method.DeclaringType;
                if (cleanAsyncMoveNext && method.Name == "MoveNext" && callerClassType?.DeclaringType != null && callerClassType.Name.IndexOf('<', StringComparison.InvariantCulture) == 0)
                {
                    // NLog.UnitTests.LayoutRenderers.CallSiteTests+<CleanNamesOfAsyncContinuations>d_3'1
                    int endIndex = callerClassType.Name.IndexOf('>', 1);
                    if (endIndex > 1)
                    {
                        callerClassType = callerClassType.DeclaringType;
                    }
                }

                if (!includeNameSpace
                    && callerClassType?.DeclaringType != null
                    && callerClassType.IsNested
                    && callerClassType.GetFirstCustomAttribute<CompilerGeneratedAttribute>() != null)
                {
                    return callerClassType.DeclaringType.Name;
                }

                string? className = includeNameSpace ? callerClassType?.FullName : callerClassType?.Name;

                if (cleanAnonymousDelegates && className != null)
                {
                    // NLog.UnitTests.LayoutRenderers.CallSiteTests+<>c__DisplayClassa
                    int index = className.IndexOf("+<>", StringComparison.Ordinal);
                    if (index >= 0)
                    {
                        className = className.Substring(0, index);
                    }
                }

                return className;
            }


            /// <summary>
            /// Returns the classname from the provided StackFrame (If not from internal assembly)
            /// </summary>
            /// <param name="stackFrame"></param>
            /// <returns>Valid class name, or empty string if assembly was internal</returns>
            public static string LookupClassNameFromStackFrame(StackFrame stackFrame)
            {
                var method = stackFrame.GetMethod();
                if (method != null && LookupAssemblyFromStackFrame(stackFrame) != null)
                {
                    string? className = GetStackFrameMethodClassName(method, true, true, true);
                    if (!string.IsNullOrEmpty(className))
                    {
                        if (!className.StartsWith("System.", StringComparison.Ordinal))
                            return className;
                    }
                    else
                    {
                        className = method.Name ?? string.Empty;
                        if (className != "lambda_method" && className != "MoveNext")
                            return className;
                    }
                }

                return string.Empty;
            }
        }
    }
}

namespace NLog.Internal
{
    internal static class ReflectionHelpers
    {
        [CanBeNull]
        public static TAttr? GetFirstCustomAttribute<TAttr>(this Type type) where TAttr : Attribute
        {
#if !NETSTANDARD1_3 && !NETSTANDARD1_5
            return Attribute.GetCustomAttributes(type, typeof(TAttr)).FirstOrDefault() as TAttr;
#else
            var typeInfo = type.GetTypeInfo();
            return typeInfo.GetCustomAttributes<TAttr>().FirstOrDefault();
#endif
        }

        public static Assembly GetAssembly(this Type type)
        {
#if !NETSTANDARD1_3 && !NETSTANDARD1_5
            return type.Assembly;
#else
            var typeInfo = type.GetTypeInfo();
            return typeInfo.Assembly;            
#endif
        }
    }
}