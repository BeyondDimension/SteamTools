using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using AndroidApplication = Android.App.Application;
using JObject = Java.Lang.Object;
using JStackTraceElement = Java.Lang.StackTraceElement;
using JThread = Java.Lang.Thread;
using JThrowable = Java.Lang.Throwable;
using NException = System.Exception;
using SQLite3 = SQLite.SQLite3;
using SQLiteException = SQLite.SQLiteException;

namespace System.Logging
{
    public sealed class ThreadUncaughtExceptionHandler : JObject, JThread.IUncaughtExceptionHandler
    {
        readonly JThread.IUncaughtExceptionHandler? mDefaultHandler;

        ThreadUncaughtExceptionHandler()
        {
            mDefaultHandler = JThread.DefaultUncaughtExceptionHandler;
            JThread.DefaultUncaughtExceptionHandler = this;
        }

        public static ThreadUncaughtExceptionHandler? Instance { get; private set; }

        public static void Initialize()
        {
            if (Instance == null) Instance = new ThreadUncaughtExceptionHandler();
        }

        static string GetLogDirPath()
        {
            var cacheDir = AndroidApplication.Context.CacheDir;
            if (cacheDir == null) throw new NullReferenceException("Application.Context.CacheDir");
            return Path.Combine(cacheDir.CanonicalPath, "catch_logs");
        }

        /// <summary>
        /// 读取上一次写入的日志
        /// </summary>
        /// <returns></returns>
        public static (DateTime fileCreationTime, string fileText) ReadLastExLog()
        {
            var dirPath = GetLogDirPath();
            var dirInfo = new DirectoryInfo(dirPath);
            if (dirInfo.Exists)
            {
                var lastExFile = dirInfo.GetFiles()
                    .Where(x => string.Equals(x.Extension, FileEx.LOG, StringComparison.OrdinalIgnoreCase))
                    .OrderByDescending(x => x.Name).FirstOrDefault();
                if (lastExFile != default)
                {
                    using var fs = lastExFile.OpenText();
                    var fileCreationTime = lastExFile.CreationTime;
                    var fileText = fs.ReadToEnd();
                    return (fileCreationTime, fileText);
                }
            }
            return default;
        }

        public static string ReadLastExLogFormat()
        {
            var temp = ReadLastExLog();
            if (temp == default) return string.Empty;
            var (time, @string) = temp;
            try
            {
                var obj = Serializable.DJSON<UncaughtExceptionArgs>(@string);
                var sb = new StringBuilder();
                sb.AppendLine($"Time: {time.ToString(DateTimeFormat.Standard)}({time.DayOfWeek.ToString2()})");
                sb.AppendLine($"Thread: {obj?.Thread?.Name}");
                sb.AppendLine($"ProcessName: {obj?.ProcessName}");
                ExceptionArgs? ex = obj?.Throwable;
                while (ex != null)
                {
                    sb.AppendLine($"ClassName: {ex.ClassName}");
                    sb.AppendLine($"Message: {ex.Message}");
                    if (ex.SQLiteExceptionResult.HasValue)
                    {
                        sb.AppendLine($"SQLiteExceptionResult: {ex.SQLiteExceptionResult.Value}({(int)ex.SQLiteExceptionResult.Value})");
                    }
                    if (ex.Message != null && !ex.Message.EndsWith(ex.NStackTrace))
                    {
                        sb.AppendLine($"StackTrace: {ex.NStackTrace}");
                    }
                    if (ex is ThrowableArgs throwable)
                    {
                        if (throwable.LocalizedMessage != ex.Message)
                        {
                            sb.AppendLine($"LocalizedMessage: {throwable.LocalizedMessage}");
                        }
                        sb.AppendLine($"JavaString: {throwable.JavaString}");
                    }
                    ex = ex.NInnerException;
                    sb.AppendLine();
                }
                return sb.ToString();
            }
            catch (NException ex)
            {
                return @string + Environment.NewLine + ex.Message;
            }
        }

        public static StackTraceElementArgs? Convert(JStackTraceElement stackTraceElement)
        {
            if (stackTraceElement == default)
            {
                return default;
            }
            return new StackTraceElementArgs
            {
                ClassName = stackTraceElement.ClassName,
                FileName = stackTraceElement.FileName,
                LineNumber = stackTraceElement.LineNumber,
                MethodName = stackTraceElement.MethodName,
                IsNativeMethod = stackTraceElement.IsNativeMethod,
                JavaString = stackTraceElement.ToString(),
            };
        }

        public static StackTraceElementArgs?[]? Convert(IEnumerable<JStackTraceElement> stackTraceElements)
        {
            return stackTraceElements?.Select(x => Convert(x))?.ToArray();
        }

        static SQLiteException? InterceptSQLiteException(NException exception)
        {
            if (exception is SQLiteException _SQLiteException)
            {
                return _SQLiteException;
            }
            else if (exception.InnerException != default)
            {
                return InterceptSQLiteException(exception.InnerException);
            }
            else
            {
                return default;
            }
        }

        static T? Convert<T>(NException exception, T? model = default) where T : ExceptionArgs, new()
        {
            if (exception == default)
            {
                return default;
            }
            if (model == default)
            {
                model = new T { };
            }
            model.Message = exception.Message;
            model.NSource = exception.Source;
            model.NHelpLink = exception.HelpLink;
            model.NHResult = exception.HResult;
            model.NStackTrace = exception.StackTrace;
            model.ClassName = exception.GetType().Name;
            model.NInnerException = Convert<ExceptionArgs>(exception.InnerException);
            return model;
        }

        public static ThrowableArgs? Convert(JThrowable? throwable)
        {
            if (throwable == default)
            {
                return default;
            }
            var result = new ThrowableArgs
            {
                LocalizedMessage = throwable.LocalizedMessage,
                StackTrace = Convert(throwable.GetStackTrace()),
                JavaString = throwable.ToString(),
            };
            var _SQLiteException = InterceptSQLiteException(throwable);
            if (_SQLiteException != default)
            {
                result.SQLiteExceptionResult = _SQLiteException.Result;
            }
            result = Convert(throwable, result);
            return result;
        }

        public static ThreadArgs? Convert(JThread? thread)
        {
            if (thread == default)
            {
                return default;
            }
            return new ThreadArgs
            {
                Id = thread.Id,
                Name = thread.Name,
                Priority = thread.Priority,
                StackTrace = Convert(thread.GetStackTrace()),
                JavaString = thread.ToString(),
            };
        }

        public static UncaughtExceptionArgs? Convert(JThread thread, JThrowable throwable)
        {
            var has_thread = thread != default;
            var has_throwable = throwable != default;
            if (!has_thread && !has_throwable)
            {
                return default;
            }
            var result = new UncaughtExceptionArgs
            {
                ProcessName = AndroidApplication.Context.GetCurrentProcessName(),
            };
            if (has_thread)
            {
                result.Thread = Convert(thread);
            }
            if (has_throwable)
            {
                result.Throwable = Convert(throwable);
            }
            return result;
        }

        void JThread.IUncaughtExceptionHandler.UncaughtException(JThread thread, JThrowable throwable)
        {
            var now = DateTime.Now;
            var dirPath = GetLogDirPath();
            var filePath = Path.Combine(dirPath, now.ToUniversalTime().Ticks + FileEx.LOG);
            var fileInfo = new FileInfo(filePath);
            if (!fileInfo.Directory.Exists)
            {
                fileInfo.Directory.Create();
            }
            if (fileInfo.Exists)
            {
                fileInfo.Delete();
            }
            var model = Convert(thread, throwable);
            if (model != null)
            {
                model.DateTime = now;
                var @string = Serializable.SJSON(model);
                using var fs = fileInfo.CreateText();
                fs.WriteLine(@string);
                fs.Flush();
            }
            mDefaultHandler?.UncaughtException(thread, throwable);
        }

        /// <summary>
        /// <see cref="JThread.IUncaughtExceptionHandler.UncaughtException(JThread, JThrowable)"/> 传入参数纪录模型类
        /// <para>https://docs.oracle.com/javase/8/docs/api/java/lang/Thread.UncaughtExceptionHandler.html#uncaughtException-java.lang.Thread-java.lang.Throwable-</para>
        /// </summary>
        public class UncaughtExceptionArgs
        {
            public ThreadArgs? Thread { get; set; }

            public ThrowableArgs? Throwable { get; set; }

            public DateTime DateTime { get; set; }

            public string? ProcessName { get; set; }
        }

        /// <summary>
        /// <see cref="JStackTraceElement"/> 模型类
        /// <para>https://docs.oracle.com/javase/8/docs/api/java/lang/StackTraceElement.html</para>
        /// </summary>
        public class StackTraceElementArgs
        {
            public string? ClassName { get; set; }

            public string? FileName { get; set; }

            public int LineNumber { get; set; }

            public string? MethodName { get; set; }

            public bool IsNativeMethod { get; set; }

            public string? JavaString { get; set; }
        }

        /// <summary>
        /// <see cref="JThread"/> 模型类
        /// <para>https://docs.oracle.com/javase/8/docs/api/java/lang/Thread.html</para>
        /// </summary>
        public class ThreadArgs
        {
            public long Id { get; set; }

            public string? Name { get; set; }

            public int Priority { get; set; }

            public StackTraceElementArgs?[]? StackTrace { get; set; }

            public string? JavaString { get; set; }
        }

        /// <summary>
        /// <see cref="NException"/> 模型类
        /// </summary>
        public class ExceptionArgs
        {
            public string? NSource { get; set; }

            public string? NHelpLink { get; set; }

            public int NHResult { get; set; }

            public string? Message { get; set; }

            public string? NStackTrace { get; set; }

            public ExceptionArgs? NInnerException { get; set; }

            public string? ClassName { get; set; }

            public SQLite3.Result? SQLiteExceptionResult { get; set; }
        }

        /// <summary>
        /// <see cref="JThrowable"/> 模型类
        /// <para>https://docs.oracle.com/javase/8/docs/api/java/lang/Throwable.html</para>
        /// </summary>
        public class ThrowableArgs : ExceptionArgs
        {
            public string? LocalizedMessage { get; set; }

            public StackTraceElementArgs?[]? StackTrace { get; set; }

            public string? JavaString { get; set; }
        }
    }
}