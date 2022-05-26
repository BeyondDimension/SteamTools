// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

#if !NET5_0_OR_GREATER

namespace System.Runtime.CompilerServices;

/// <summary>
/// An attribute that allows parameters to receive the expression of other parameters.
/// </summary>
/// <remarks>Internal copy from the BCL attribute.</remarks>
[AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false, Inherited = false)]
internal sealed class CallerArgumentExpressionAttribute : Attribute
{
    /// <summary>
    /// Initializes a new instance of the <see cref="CallerArgumentExpressionAttribute"/> class.
    /// </summary>
    /// <param name="parameterName">The condition parameter value.</param>
    public CallerArgumentExpressionAttribute(string parameterName)
    {
        ParameterName = parameterName;
    }

    /// <summary>
    /// Gets the parameter name the expression is retrieved from.
    /// </summary>
    public string ParameterName { get; }
}

#endif