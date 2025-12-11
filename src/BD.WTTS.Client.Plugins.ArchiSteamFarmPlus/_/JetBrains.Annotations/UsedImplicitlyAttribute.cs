// ReSharper disable once CheckNamespace
namespace JetBrains.Annotations;

[AttributeUsage(AttributeTargets.All)]
sealed class UsedImplicitlyAttribute : Attribute
{
    public ImplicitUseKindFlags UseKindFlags { get; }

    public ImplicitUseTargetFlags TargetFlags { get; }

    public UsedImplicitlyAttribute()
        : this(ImplicitUseKindFlags.Default, ImplicitUseTargetFlags.Default)
    {
    }

    public UsedImplicitlyAttribute(ImplicitUseKindFlags useKindFlags)
        : this(useKindFlags, ImplicitUseTargetFlags.Default)
    {
    }

    public UsedImplicitlyAttribute(ImplicitUseTargetFlags targetFlags)
        : this(ImplicitUseKindFlags.Default, targetFlags)
    {
    }

    public UsedImplicitlyAttribute(ImplicitUseKindFlags useKindFlags, ImplicitUseTargetFlags targetFlags)
    {
        UseKindFlags = useKindFlags;
        TargetFlags = targetFlags;
    }
}

/// <summary>
/// Specifies the details of implicitly used symbol when it is marked
/// with <see cref="MeansImplicitUseAttribute"/> or <see cref="UsedImplicitlyAttribute"/>.
/// </summary>
[Flags]
enum ImplicitUseKindFlags
{
    Default = Access | Assign | InstantiatedWithFixedConstructorSignature,

    /// <summary>Only entity marked with attribute considered used.</summary>
    Access = 1,

    /// <summary>Indicates implicit assignment to a member.</summary>
    Assign = 2,

    /// <summary>
    /// Indicates implicit instantiation of a type with fixed constructor signature.
    /// That means any unused constructor parameters won't be reported as such.
    /// </summary>
    InstantiatedWithFixedConstructorSignature = 4,

    /// <summary>Indicates implicit instantiation of a type.</summary>
    InstantiatedNoFixedConstructorSignature = 8,
}

/// <summary>
/// Specifies what is considered to be used implicitly when marked
/// with <see cref="MeansImplicitUseAttribute"/> or <see cref="UsedImplicitlyAttribute"/>.
/// </summary>
[Flags]
enum ImplicitUseTargetFlags
{
    Default = Itself,

    Itself = 1,

    /// <summary>Members of the type marked with the attribute are considered used.</summary>
    Members = 2,

    /// <summary> Inherited entities are considered used. </summary>
    WithInheritors = 4,

    /// <summary>Entity marked with the attribute and all its members considered used.</summary>
    WithMembers = Itself | Members,
}