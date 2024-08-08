namespace BD;

/// <summary>
/// https://github.com/BeyondDimension/SteamTools/compare/diff_mobius_tag...develop?expand=1
/// </summary>
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
