// ReSharper disable once CheckNamespace
namespace BD.WTTS.Models;

public class IntStatInfo : StatInfo
{
    public int OriginalValue { get; set; }

    public int IntValue { get; set; }

    public int MinValue { get; set; }

    public int MaxValue { get; set; }

    public int MaxChange { get; set; }

    public bool IncrementOnly { get; set; }

    public int DefaultValue { get; set; }

    public override object Value
    {
        get => IntValue;
        set
        {
            var b = int.TryParse((string)value, out int i);
            if (b)
            {
                if ((Permission & 2) != 0 && IntValue != i)
                {
                    //this.IntValue = this.IntValue;
                }
                else
                {
                    IntValue = i;
                }
            }
        }
    }

    public override bool IsModified => IntValue != OriginalValue;
}