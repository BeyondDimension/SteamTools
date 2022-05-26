namespace System;

/// <summary>
/// 小数数字转换为整数数字格式
/// </summary>
public enum NumberToInt32Format
{
    /// <summary>
    /// 四舍五入
    /// </summary>
    RoundAwayFromZero,

    /// <summary>
    /// 五舍六入
    /// </summary>
    Round,

    /// <summary>
    /// 仅取整数
    /// </summary>
    Floor,

    /// <summary>
    /// 进一法
    /// </summary>
    Ceiling,
}

/// <summary>
/// Enum 扩展 <see cref="NumberToInt32Format"/>
/// </summary>
public static partial class NumberToInt32FormatEnumExtensions
{
    public static int ToInt32(this float value, NumberToInt32Format format = NumberToInt32Format.Floor) => format switch
    {
        NumberToInt32Format.RoundAwayFromZero => (int)MathF.Round(value, MidpointRounding.AwayFromZero),
        NumberToInt32Format.Round => (int)MathF.Round(value),
        NumberToInt32Format.Floor => (int)MathF.Floor(value),
        NumberToInt32Format.Ceiling => (int)MathF.Ceiling(value),
        _ => throw new ArgumentOutOfRangeException(nameof(format), format, null),
    };

    public static int ToInt32(this double value, NumberToInt32Format format = NumberToInt32Format.Floor) => format switch
    {
        NumberToInt32Format.RoundAwayFromZero => (int)Math.Round(value, MidpointRounding.AwayFromZero),
        NumberToInt32Format.Round => (int)Math.Round(value),
        NumberToInt32Format.Floor => (int)Math.Floor(value),
        NumberToInt32Format.Ceiling => (int)Math.Ceiling(value),
        _ => throw new ArgumentOutOfRangeException(nameof(format), format, null),
    };
}