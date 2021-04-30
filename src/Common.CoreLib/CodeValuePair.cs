//using Newtonsoft.Json;
//using Newtonsoft.Json.Linq;
//using System.Collections.Generic;

//namespace System
//{
//    /// <summary>
//    /// 利用Json字符串实现的一种 状态码(code) 对应 模型类(value) 动态解析 switch case 模式，数据类似与 <see cref="KeyValuePair{TKey, TValue}"/>
//    /// </summary>
//    public static class CodeValuePair
//    {
//        public const string code = "code";

//        public const string value = "value";

//        public static string Create<TCode>(TCode code, object? value) where TCode : struct, IConvertible
//        {
//            var result = new Dictionary<string, object?>
//            {
//                { CodeValuePair.code, code.ConvertToInt32() },
//                { CodeValuePair.value, value }
//            };
//            var jsonStr = JsonConvert.SerializeObject(result);
//            return jsonStr;
//        }

//        public static JObject GetValue(JObject jObject)
//        {
//            var value = jObject.Value<JObject>(CodeValuePair.value);
//            return value;
//        }

//        public static Action<JObject> Switch<TValue>(Action<TValue?> action) => jObject =>
//        {
//            var value = GetValue(jObject).ToObject<TValue>();
//            action?.Invoke(value);
//        };

//        public static TResult? Switch<TCode, TValue, TResult>(
//            string jsonStr,
//            IReadOnlyDictionary<TCode, TValue> keyValuePairs,
//            Func<TCode, TValue, JObject, TResult?> @delegate)
//            where TCode : struct, IConvertible
//        {
//            if (!string.IsNullOrWhiteSpace(jsonStr))
//            {
//                var jObject = JObject.Parse(jsonStr);
//                var key = jObject.Value<int>(code).ConvertToEnum<TCode>();
//                if (keyValuePairs.ContainsKey(key))
//                {
//                    var value = keyValuePairs[key];
//                    return @delegate.Invoke(key, value, jObject);
//                }
//            }
//            return default;
//        }

//        public static void Switch<TCode>(
//            string jsonStr,
//            IReadOnlyDictionary<TCode, Action<JObject>> keyValuePairs)
//            where TCode : struct, IConvertible
//        {
//            Switch(jsonStr, keyValuePairs, Invoke);
//            static object? Invoke(TCode key, Action<JObject> value, JObject @object)
//            {
//                value?.Invoke(@object);
//                return default;
//            }
//        }
//    }

//#if DEBUG

//    [Obsolete("", true)]
//    public static partial class Default
//    {
//        [Obsolete("use CodeValuePair", true)]
//        public static class Keys
//        {
//            public const string code = CodeValuePair.code;

//            public const string value = CodeValuePair.value;

//            public static JObject GetValue(JObject jObject) => CodeValuePair.GetValue(jObject);

//            public static Action<JObject> Switch<TValue>(Action<TValue?> arg) => CodeValuePair.Switch(arg);
//        }
//    }

//#endif
//}