// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// https://github.com/dotnet/runtime/blob/v6.0.5/src/libraries/System.Private.CoreLib/src/System/Diagnostics/CodeAnalysis/DynamicDependencyAttribute.cs

#if !NET5_0_OR_GREATER
namespace System.Diagnostics.CodeAnalysis;

/// <summary>
/// States a dependency that one member has on another.
/// </summary>
/// <remarks>
/// This can be used to inform tooling of a dependency that is otherwise not evident purely from
/// metadata and IL, for example a member relied on via reflection.
/// </remarks>
[AttributeUsage(
    AttributeTargets.Constructor | AttributeTargets.Field | AttributeTargets.Method,
    AllowMultiple = true, Inherited = false)]
public sealed class DynamicDependencyAttribute : Attribute
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DynamicDependencyAttribute"/> class
    /// with the specified signature of a member on the same type as the consumer.
    /// </summary>
    /// <param name="memberSignature">The signature of the member depended on.</param>
    public DynamicDependencyAttribute(string memberSignature)
    {
        MemberSignature = memberSignature;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="DynamicDependencyAttribute"/> class
    /// with the specified signature of a member on a <see cref="System.Type"/>.
    /// </summary>
    /// <param name="memberSignature">The signature of the member depended on.</param>
    /// <param name="type">The <see cref="System.Type"/> containing <paramref name="memberSignature"/>.</param>
    public DynamicDependencyAttribute(string memberSignature, Type type)
    {
        MemberSignature = memberSignature;
        Type = type;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="DynamicDependencyAttribute"/> class
    /// with the specified signature of a member on a type in an assembly.
    /// </summary>
    /// <param name="memberSignature">The signature of the member depended on.</param>
    /// <param name="typeName">The full name of the type containing the specified member.</param>
    /// <param name="assemblyName">The assembly name of the type containing the specified member.</param>
    public DynamicDependencyAttribute(string memberSignature, string typeName, string assemblyName)
    {
        MemberSignature = memberSignature;
        TypeName = typeName;
        AssemblyName = assemblyName;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="DynamicDependencyAttribute"/> class
    /// with the specified types of members on a <see cref="System.Type"/>.
    /// </summary>
    /// <param name="memberTypes">The types of members depended on.</param>
    /// <param name="type">The <see cref="System.Type"/> containing the specified members.</param>
    public DynamicDependencyAttribute(DynamicallyAccessedMemberTypes memberTypes, Type type)
    {
        MemberTypes = memberTypes;
        Type = type;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="DynamicDependencyAttribute"/> class
    /// with the specified types of members on a type in an assembly.
    /// </summary>
    /// <param name="memberTypes">The types of members depended on.</param>
    /// <param name="typeName">The full name of the type containing the specified members.</param>
    /// <param name="assemblyName">The assembly name of the type containing the specified members.</param>
    public DynamicDependencyAttribute(DynamicallyAccessedMemberTypes memberTypes, string typeName, string assemblyName)
    {
        MemberTypes = memberTypes;
        TypeName = typeName;
        AssemblyName = assemblyName;
    }

    /// <summary>
    /// Gets the signature of the member depended on.
    /// </summary>
    /// <remarks>
    /// Either <see cref="MemberSignature"/> must be a valid string or <see cref="MemberTypes"/>
    /// must not equal <see cref="DynamicallyAccessedMemberTypes.None"/>, but not both.
    /// </remarks>
    public string? MemberSignature { get; }

    /// <summary>
    /// Gets the <see cref="DynamicallyAccessedMemberTypes"/> which specifies the type
    /// of members depended on.
    /// </summary>
    /// <remarks>
    /// Either <see cref="MemberSignature"/> must be a valid string or <see cref="MemberTypes"/>
    /// must not equal <see cref="DynamicallyAccessedMemberTypes.None"/>, but not both.
    /// </remarks>
    public DynamicallyAccessedMemberTypes MemberTypes { get; }

    /// <summary>
    /// Gets the <see cref="System.Type"/> containing the specified member.
    /// </summary>
    /// <remarks>
    /// If neither <see cref="Type"/> nor <see cref="TypeName"/> are specified,
    /// the type of the consumer is assumed.
    /// </remarks>
    public Type? Type { get; }

    /// <summary>
    /// Gets the full name of the type containing the specified member.
    /// </summary>
    /// <remarks>
    /// If neither <see cref="Type"/> nor <see cref="TypeName"/> are specified,
    /// the type of the consumer is assumed.
    /// </remarks>
    public string? TypeName { get; }

    /// <summary>
    /// Gets the assembly name of the specified type.
    /// </summary>
    /// <remarks>
    /// <see cref="AssemblyName"/> is only valid when <see cref="TypeName"/> is specified.
    /// </remarks>
    public string? AssemblyName { get; }

    /// <summary>
    /// Gets or sets the condition in which the dependency is applicable, e.g. "DEBUG".
    /// </summary>
    public string? Condition { get; set; }
}

public enum DynamicallyAccessedMemberTypes
{
    /// <summary>
    /// Specifies no members.
    /// </summary>
    None = 0,

    /// <summary>
    /// Specifies the default, parameterless public constructor.
    /// </summary>
    PublicParameterlessConstructor = 0x0001,

    /// <summary>
    /// Specifies all public constructors.
    /// </summary>
    PublicConstructors = 0x0002 | PublicParameterlessConstructor,

    /// <summary>
    /// Specifies all non-public constructors.
    /// </summary>
    NonPublicConstructors = 0x0004,

    /// <summary>
    /// Specifies all public methods.
    /// </summary>
    PublicMethods = 0x0008,

    /// <summary>
    /// Specifies all non-public methods.
    /// </summary>
    NonPublicMethods = 0x0010,

    /// <summary>
    /// Specifies all public fields.
    /// </summary>
    PublicFields = 0x0020,

    /// <summary>
    /// Specifies all non-public fields.
    /// </summary>
    NonPublicFields = 0x0040,

    /// <summary>
    /// Specifies all public nested types.
    /// </summary>
    PublicNestedTypes = 0x0080,

    /// <summary>
    /// Specifies all non-public nested types.
    /// </summary>
    NonPublicNestedTypes = 0x0100,

    /// <summary>
    /// Specifies all public properties.
    /// </summary>
    PublicProperties = 0x0200,

    /// <summary>
    /// Specifies all non-public properties.
    /// </summary>
    NonPublicProperties = 0x0400,

    /// <summary>
    /// Specifies all public events.
    /// </summary>
    PublicEvents = 0x0800,

    /// <summary>
    /// Specifies all non-public events.
    /// </summary>
    NonPublicEvents = 0x1000,

    /// <summary>
    /// Specifies all interfaces implemented by the type.
    /// </summary>
    Interfaces = 0x2000,

    /// <summary>
    /// Specifies all members.
    /// </summary>
    All = ~None
}
#endif