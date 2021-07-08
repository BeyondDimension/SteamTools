using Android.Runtime;
using System.Common;
using System.Diagnostics;
using System.Reflection;
using JClass = Java.Lang.Class;
using JEnum = Java.Lang.Enum;
using JFile = Java.IO.File;
using JString = Java.Lang.String;
using JThrowable = Java.Lang.Throwable;

// ReSharper disable once CheckNamespace
namespace System
{
    public static class JavaExtensions
    {
        /// <summary>
        /// 将 <see cref="Type"/> 转换为 <see cref="Java.Lang.Class"/> 对象。
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static JClass GetJClass(this Type type) => JClass.FromType(type);

        /// <summary>
        /// 文件是否存在。
        /// </summary>
        /// <param name="file"></param>
        /// <returns></returns>
        public static bool HasFile(this JFile? file) => file != null && file.IsFile && file.Exists();

        public static string GetJavaEnumName(this JEnum @enum)
        {
            var type = @enum.GetType();
            var typeName = type.Namespace + Constants.DOT + type.Name;
            var enumName = @enum.Name();
            try
            {
                var properties = type.GetProperties(BindingFlags.Public | BindingFlags.Static);
                foreach (var property in properties)
                {
                    RegisterAttribute? registerAttribute = null;
                    try
                    {
                        registerAttribute = property.GetCustomAttribute<RegisterAttribute>(false);
                    }
                    catch
                    {
                        continue;
                    }
                    if (registerAttribute != null)
                    {
                        if (registerAttribute.Name == enumName)
                        {
                            return typeName + Constants.DOT + property.Name;
                        }
                    }
                }
            }
            catch
            {
                try
                {
                    var name = enumName.ToCharArray();
                    var index = Constants.LowerCaseLetters.IndexOf(name[0]);
                    if (index >= 0)
                    {
                        name[0] = Constants.UpperCaseLetters[index];
                        return typeName + Constants.DOT + new string(name);
                    }
                }
                catch
                {
                }
            }
            return typeName + Constants.DOT + enumName;
        }

        /// <summary>
        /// 仅Debug模式时，调用 <see cref="JThrowable.PrintStackTrace"/>
        /// </summary>
        /// <param name="throwable"></param>
        [Conditional("DEBUG")]
        public static void PrintStackTraceWhenDebug(this JThrowable throwable)
            => throwable.PrintStackTrace();

        /// <summary>
        /// 将 <see cref="string"/> 转换为 <see cref="JString"/>
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        public static JString ToJavaString(this string s) => new(s);

        /// <inheritdoc cref="ToJavaString(string)"/>
        public static JString? ToJavaString_Nullable(this string? s)
            => s == null ? null : new JString(s);

        /// <summary>
        /// Java中的SubString函数实现
        /// </summary>
        /// <param name="s"></param>
        /// <param name="beginIndex"></param>
        /// <param name="endIndex"></param>
        /// <returns></returns>
        public static string JavaSubstring(this string s, int beginIndex, int endIndex)
            => s[beginIndex..endIndex];
    }
}