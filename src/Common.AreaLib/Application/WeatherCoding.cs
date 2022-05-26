//using System.Collections.Generic;

//namespace System.Application;

///// <summary>
///// 高德地图天气现象表
///// <para>web api: https://lbs.amap.com/api/webservice/guide/tools/weather-code </para>
///// <para>android sdk: https://lbs.amap.com/api/android-sdk/guide/map-tools/weather-code </para>
///// </summary>
//public static class WeatherCoding
//{
//    const string TAG = "WeatherCoding";

//    public const string 未知 = "未知";

//    /// <inheritdoc cref="WeatherCoding"/>
//    public enum Value
//    {
//        未知 = 0,
//        晴,
//        多云,
//        阴,
//        阵雨,
//        雷阵雨,
//        雷阵雨并伴有冰雹,
//        雨夹雪,
//        小雨,
//        中雨,
//        大雨,
//        暴雨,
//        大暴雨,
//        特大暴雨,
//        阵雪,
//        小雪,
//        中雪,
//        大雪,
//        暴雪,
//        雾,
//        冻雨,
//        沙尘暴,
//        小雨转中雨,
//        中雨转大雨,
//        大雨转暴雨,
//        暴雨转大暴雨,
//        大暴雨转特大暴雨,
//        小雪转中雪,
//        中雪转大雪,
//        大雪转暴雪,
//        浮尘,
//        扬沙,
//        强沙尘暴,
//        飑,
//        龙卷风,
//        弱高吹雪,
//        轻雾,
//        霾,

//        少云,
//        晴间多云,
//        有风,
//        平静,
//        微风,
//        和风,
//        清风,
//        强风,
//        疾风,
//        大风,
//        烈风,
//        风暴,
//        狂爆风,
//        飓风,
//        热带风暴,
//        强阵雨,
//        强雷阵雨,
//        极端降雨,
//        细雨,
//        雨,
//        雨雪天气,
//        阵雨夹雪,
//        雪,
//        浓雾,
//        强浓雾,
//        大雾,
//        特强浓雾,
//        中度霾,
//        重度霾,
//        严重霾,
//        热,
//        冷,
//    }

//    /// <summary>
//    /// 某些值文字太长，需要使用对应一个短文字值的
//    /// </summary>
//    static readonly IReadOnlyDictionary<Value, Value> short_mapping = new Dictionary<Value, Value>
//    {
//        { Value.雷阵雨并伴有冰雹 , Value.雷阵雨 },
//        { Value.特大暴雨 , Value.大暴雨 },
//        { Value.小雨转中雨 ,  Value.小雨 },
//        { Value.中雨转大雨 ,  Value.中雨 },
//        { Value.大雨转暴雨 ,  Value.大雨 },
//        { Value.暴雨转大暴雨 ,  Value.暴雨 },
//        { Value.大暴雨转特大暴雨 ,  Value.大暴雨 },
//        { Value.小雪转中雪 ,  Value.小雪 },
//        { Value.中雪转大雪 ,  Value.中雪 },
//        { Value.大雪转暴雪 ,  Value.暴雪 },
//        { Value.强沙尘暴 ,  Value.沙尘暴 },
//        { Value.弱高吹雪 ,  Value.小雪 },
//        { Value.晴间多云 ,  Value.多云 },
//    };

//    static readonly IReadOnlyDictionary<string, Value> str_parse_mapping = new Dictionary<string, Value>(StringComparer.Ordinal)
//    {
//        { "强风/劲风", Value.强风 },
//        { "劲风", Value.强风 },
//        { "毛毛雨/细雨", Value.细雨 },
//        { "毛毛雨", Value.细雨 },
//        { "小雨-中雨", Value.小雨转中雨 },
//        { "中雨-大雨", Value.中雨转大雨 },
//        { "大雨-暴雨", Value.大雨转暴雨 },
//        { "暴雨-大暴雨", Value.暴雨转大暴雨 },
//        { "大暴雨-特大暴雨", Value.大暴雨转特大暴雨 },
//        { "小雪-中雪", Value.小雪转中雪 },
//        { "中雪-大雪", Value.中雪转大雪 },
//        { "大雪-暴雪", Value.大雪转暴雪 },
//    };

//    /// <summary>
//    /// 短文字值组排除项
//    /// </summary>
//    static readonly Value[] short_values_excluded = new[] {
//        Value.未知,
//        Value.热,
//        Value.冷,
//    };

//    /// <summary>
//    /// 获取短文字值
//    /// </summary>
//    /// <param name="value"></param>
//    /// <returns></returns>
//    public static Value ToShort(this Value value)
//    {
//        if (short_mapping.ContainsKey(value)) return short_mapping[value];
//        return value;
//    }

//    /// <summary>
//    /// 获取短文字值字符串
//    /// </summary>
//    /// <param name="value"></param>
//    /// <returns></returns>
//    public static string ToStringShort(this Value value) => value.ToShort().ToString();

//    /// <summary>
//    /// 获取所有的短文字值
//    /// </summary>
//    /// <returns></returns>
//    public static IEnumerable<Value> GetValuesShort()
//    {
//        var values = (Value[])Enum.GetValues(typeof(WeatherCoding));
//        foreach (var value in values)
//        {
//            // 忽略排除项
//            if (short_values_excluded.Contains(value)) continue;
//            // 忽略值文字过长的
//            if (short_mapping.ContainsKey(value)) continue;
//            yield return value;
//        }
//    }

//    /// <summary>
//    /// 获取所有的短文字值与传入的当前天气
//    /// </summary>
//    /// <param name="currentWeather"></param>
//    /// <returns></returns>
//    public static List<Value?> GetValues(Value? currentWeather)
//    {
//        var result = new List<Value?> { currentWeather };
//        var values = GetValuesShort().ToArray();
//        Value? continueValue = null;
//        if (currentWeather.HasValue)
//        {
//            var shortValue = currentWeather.Value.ToShort();
//            if (values.Contains(shortValue))
//            {
//                continueValue = shortValue;
//            }
//        }
//        foreach (var value in values)
//        {
//            if (continueValue.HasValue && continueValue.Value == value) continue;
//            result.Add(value);
//        }
//        return result;
//    }

//    public static Value Parse(string value)
//    {
//        if (str_parse_mapping.ContainsKey(value)) return str_parse_mapping[value];
//        if (!Enum.TryParse(value, out Value result))
//        {
//            Log.Error(TAG, "WeatherCoding Enum.TryParse Fail value: {0}", value);
//            return Value.未知;
//        }
//        return result;
//    }
//}