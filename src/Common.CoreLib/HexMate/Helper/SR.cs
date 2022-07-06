namespace HexMate;

static class SR
{
    internal const string Arg_EnumIllegalVal = "Illegal enum value: {0}.";
    internal const string ArgumentOutOfRange_Index = "Index was out of range. Must be non-negative and less than the size of the collection.";
    internal const string ArgumentOutOfRange_GenericPositive = "Value must be positive.";
    internal const string ArgumentOutOfRange_OffsetLength = "Offset and length must refer to a position in the string.";
    internal const string ArgumentOutOfRange_OffsetOut = "Either offset did not refer to a position in the string, or there is an insufficient length of destination character array.";
    internal const string Format_BadHexChar = "The input is not a valid hex string as it contains a non-hex character.";

    internal static string Format(string pattern, int arg0)
    {
        return string.Format(pattern, arg0.ToString());
    }
}