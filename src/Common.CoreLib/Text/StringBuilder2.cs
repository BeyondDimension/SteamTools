namespace System.Text;

public sealed class StringBuilder2 : IStringBuilder
{
    readonly StringBuilder builder;

    public StringBuilder2() : this(new())
    {

    }

    public StringBuilder2(StringBuilder builder)
    {
        this.builder = builder;
    }

    IStringBuilder IStringBuilder<IStringBuilder>.Append(string? value)
    {
        builder.Append(value);
        return this;
    }

    public override string ToString() => builder.ToString();
}
