namespace BD.WTTS.Enums;

public enum EncodingType : byte
{
    /// <summary>
    /// 自动，在 Windows 上使用 <see cref="UTF8WithBOM"/>，在其他操作系统上使用 <see cref="UTF8"/>
    /// </summary>
    Auto,

    /// <summary>
    /// (仅 Windows)系统的活动代码页并创建 Encoding 与其对应的对象。 
    /// 活动代码页可能是 ANSI 代码页，其中包括 ASCII 字符集以及不同于代码页的其他字符。
    /// <para></para>
    /// 在非 Windows 上此项与 <see cref="UTF8"/> 行为一致。
    /// </summary>
    ANSICodePage,

    UTF8,

    UTF8WithBOM,
}
