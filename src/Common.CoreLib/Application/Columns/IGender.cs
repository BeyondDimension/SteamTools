namespace System.Application.Columns;

/// <inheritdoc cref="Enums.Gender"/>
public interface IGender
{
    /// <inheritdoc cref="Enums.Gender"/>
    Gender Gender { get; set; }
}

/// <inheritdoc cref="Enums.Gender"/>
public interface IReadOnlyGender
{
    /// <inheritdoc cref="Enums.Gender"/>
    Gender Gender { get; }
}

#if DEBUG

[Obsolete("use IGender", true)]
public interface IPropertyGender { }

#endif