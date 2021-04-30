using Android.OS;
using Android.Runtime;
using Java.Lang;
using Java.Util;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using Exception = System.Exception;
using JRuntime = Java.Lang.Runtime;
using String = Java.Lang.String;

// ReSharper disable once CheckNamespace
namespace Android.OS
{
    /// <summary>
    /// 通过 Java 反射调用 android.os.SystemProperties
    /// <para>https://android.googlesource.com/platform/frameworks/base/+/master/core/java/android/os/SystemProperties.java</para>
    /// </summary>
    internal static class SystemProperties
    {
        [Obsolete]
        public static string? Get(string key) => lazy_native_get.Value.Invoke(key);

        [Obsolete]
        public static string? Get(string key, string? def) => lazy_native_get2.Value.Invoke(key, def);

        [Obsolete]
        public static int GetInt(string key, int def) => lazy_native_get_int.Value.Invoke(key, def);

        [Obsolete]
        public static long GetLong(string key, long def) => lazy_native_get_long.Value.Invoke(key, def);

        [Obsolete]
        public static bool GetBoolean(string key, bool def) => lazy_native_get_boolean.Value.Invoke(key, def);

        [Obsolete]
        public static void Set(string key, string val) => lazy_native_set.Value.Invoke(key, val);

        #region 反射实现

#pragma warning disable IDE1006 // 命名样式

        static TResult reflection_call_convert<TResult>(Func<Java.Lang.Object?, TResult> convert, string name, IEnumerable<Type> types, params Java.Lang.Object?[] args)
        {
            var t = reflection_call(name, types, args);
            return convert(t);
        }

        static TResult reflection_call_convert<T, TResult>(Func<T?, TResult> convert, string name, IEnumerable<Type> types, params Java.Lang.Object?[] args) where T : class, IJavaObject
        {
            var t = reflection_call(name, types, args).JavaCast<T>();
            return convert(t);
        }

        static Java.Lang.Object? reflection_call(string name, IEnumerable<Type> types, params Java.Lang.Object?[] args)
        {
            var @class = lazy_native_class.Value;
            var parameterTypes = types.Select(x => Class.FromType(x)).ToArray();
            var method = @class.GetMethod(name, parameterTypes);
#pragma warning disable CS8620 // 由于引用类型的可为 null 性差异，实参不能用于形参。
            var resultObject = method.Invoke(@class, args);
#pragma warning restore CS8620 // 由于引用类型的可为 null 性差异，实参不能用于形参。
            return resultObject;
        }

        static string? convert_string_to_string(String? s) => s?.ToString();

        static int convert_object_to_int(Java.Lang.Object? o) => o == default ? default : (int)o;

        static long convert_object_to_long(Java.Lang.Object? o) => o == default ? default : (long)o;

        static bool convert_object_to_bool(Java.Lang.Object? o) => o != default && (bool)o;

        static readonly Lazy<Class> lazy_native_class
            = new(() => Class.ForName("android.os.SystemProperties"));

        static readonly Lazy<Func<string, string?>> lazy_native_get
           = new(() => (key)
           => reflection_call_convert<String, string?>(convert_string_to_string, "get", new[] { typeof(String) }, new String(key)));

        static readonly Lazy<Func<string, string?, string?>> lazy_native_get2
           = new(() => (key, def)
           => reflection_call_convert<String, string?>(convert_string_to_string, "get", new[] { typeof(String), typeof(String) }, key.ToJavaString(), def.ToJavaString_Nullable()));

        static readonly Lazy<Func<string, int, int>> lazy_native_get_int
           = new(() => (key, def)
           => reflection_call_convert(convert_object_to_int, "getInt", new[] { typeof(String), typeof(int) }, new String(key), def));

        static readonly Lazy<Func<string, long, long>> lazy_native_get_long
           = new(() => (key, def)
           => reflection_call_convert(convert_object_to_long, "getLong", new[] { typeof(String), typeof(long) }, new String(key), def));

        static readonly Lazy<Func<string, bool, bool>> lazy_native_get_boolean
            = new(() => (key, def)
           => reflection_call_convert(convert_object_to_bool, "getBoolean", new[] { typeof(String), typeof(bool) }, new String(key), def));

        static readonly Lazy<Action<string, string>> lazy_native_set
           = new(() => (key, val)
           => reflection_call("set", new[] { typeof(String), typeof(String) }, new String(key), new String(val)));

#pragma warning restore IDE1006 // 命名样式

        #endregion

        [Conditional("DEBUG")]
        static void LogError(this Exception ex, string message)
            => DI.Get<ILoggerFactory>().CreateLogger("SystemProperties").LogError(ex, message);

        static readonly Lazy<Properties?> mBuildProperties
            = new(GetBuildProperties);

        const string planA = "/system/build.prop";
        const string planB = "getprop";

        public static Properties? GetBuildPropertiesByPlanA()
        {
            var fileInfo = new FileInfo(planA);
            if (fileInfo.Exists)
            {
                var properties = new Properties();
                properties.Load(fileInfo.OpenRead());
                return properties;
            }
            return default;
        }

        public static Properties? GetBuildPropertiesByPlanB()
        {
            var runtime = JRuntime.GetRuntime();
            if (runtime != null)
            {
                using var getpropProcess = runtime.Exec(planB);
                if (getpropProcess != null)
                {
                    using var streamReader = new Java.IO.InputStreamReader(getpropProcess.InputStream);
                    using var buildProperties = new Java.IO.BufferedReader(streamReader);
                    var properties = new Properties();
                    string? line;
                    while ((line = buildProperties.ReadLine()) != null)
                    {
                        var key_value = line
                            .Replace("[", string.Empty)
                            .Replace("]", string.Empty)
                            .Split(new char[] { ':' }, StringSplitOptions.RemoveEmptyEntries)
                            .Select(x => x.Trim()).ToArray();
                        if (key_value.Length == 2)
                        {
                            properties.SetProperty(key_value[0], key_value[1]);
                        }
                    }
                    getpropProcess.WaitFor();
                    buildProperties.Close();
                    return properties;
                }
            }
            return default;
        }

        internal static Properties? TryGetBuildPropertiesByPlanA()
        {
            try
            {
                return GetBuildPropertiesByPlanA();
            }
            catch (Exception ex)
            {
                LogError(ex, planA);
            }
            return default;
        }

        internal static Properties? TryGetBuildPropertiesByPlanB()
        {
            try
            {
                return GetBuildPropertiesByPlanB();
            }
            catch (Exception ex)
            {
                LogError(ex, planB);
            }
            return default;
        }

        static Properties? GetBuildProperties()
        {
            Properties? properties;
            if (Build.VERSION.SdkInt < BuildVersionCodes.O)
            {
                properties = TryGetBuildPropertiesByPlanA();
                if (properties == null) properties = TryGetBuildPropertiesByPlanB();
            }
            else
            {
                properties = TryGetBuildPropertiesByPlanB();
                if (properties == null) properties = TryGetBuildPropertiesByPlanA();
            }
            return properties;
        }

        public static Properties? BuildProperties => mBuildProperties.Value;

        public static string Print(this Properties? properties)
        {
            if (properties != null)
            {
                const string start = "Count = ";
                var keys = properties.StringPropertyNames();
                if (keys != null)
                {
                    var sb = new System.Text.StringBuilder(start);
                    sb.AppendLine(keys.Count.ToString());
                    foreach (var key in keys)
                    {
                        sb.Append(key);
                        sb.Append(" = ");
                        sb.AppendLine(properties.GetProperty(key));
                    }
                    return sb.ToString();
                }
                else
                {
                    return start;
                }
            }
            return string.Empty;
        }

        public static bool TryGet(string key, [NotNullWhen(true)] out string? value)
        {
            try
            {
#pragma warning disable CS0612 // 类型或成员已过时
                value = Get(key);
#pragma warning restore CS0612 // 类型或成员已过时
                return !string.IsNullOrWhiteSpace(value);
            }
            catch (Exception ex1)
            {
                ex1.LogError("SystemProperties.Get");
                try
                {
                    value = BuildProperties?.GetProperty(key);
                    return !string.IsNullOrWhiteSpace(value);
                }
                catch (Exception ex2)
                {
                    ex2.LogError("BuildProperties.GetProperty");
                }
            }
            value = null;
            return false;
        }
    }
}