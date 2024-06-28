namespace BD;

[AttributeUsage(AttributeTargets.All, Inherited = false)]
public sealed class MobiusAttribute : Attribute
{
    public MobiusAttribute()
    {

    }

    public MobiusAttribute(string str)
    {

    }

    public bool Obsolete { get; set; }
}
