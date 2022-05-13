// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.
// https://github.com/CommunityToolkit/dotnet/blob/v8.0.0-preview3/CommunityToolkit.Mvvm.SourceGenerators/Attributes/NotNullWhenAttribute.cs

namespace System.Diagnostics.CodeAnalysis;

/// <summary>Specifies that when a method returns <see cref="ReturnValue"/>, the parameter will not be null even if the corresponding type allows it.</summary>
[AttributeUsage(AttributeTargets.Parameter, Inherited = false)]
internal sealed class NotNullWhenAttribute : Attribute
{
    /// <summary>
    /// Initializes a new instance of the <see cref="NotNullWhenAttribute"/> class.
    /// </summary>
    /// <param name="returnValue">The return value condition. If the method returns this value, the associated parameter will not be null.</param>
    public NotNullWhenAttribute(bool returnValue)
    {
        ReturnValue = returnValue;
    }

    /// <summary>
    /// Gets a value indicating whether the annotated parameter will be null depending on the return value.
    /// </summary>
    public bool ReturnValue { get; }
}