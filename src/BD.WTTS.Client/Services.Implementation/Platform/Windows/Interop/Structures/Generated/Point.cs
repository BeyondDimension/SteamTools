// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.
// https://github.com/dotnet/wpf/blob/v6.0.0-rc.2.21501.7/src/Microsoft.DotNet.Wpf/src/WindowsBase/System/Windows/Generated/Point.cs
//
//
// This file was generated, please do not edit it directly.
//
// Please see MilCodeGen.html for more information.

// ReSharper disable once CheckNamespace
namespace System.Windows;

[Serializable]
partial struct PointD : IFormattable
{
    //------------------------------------------------------
    //
    //  Public Methods
    //
    //------------------------------------------------------

    #region Public Methods

    /// <summary>
    /// Compares two Point instances for exact equality.
    /// Note that double values can acquire error when operated upon, such that
    /// an exact comparison between two values which are logically equal may fail.
    /// Furthermore, using this equality operator, Double.NaN is not equal to itself.
    /// </summary>
    /// <returns>
    /// bool - true if the two Point instances are exactly equal, false otherwise
    /// </returns>
    /// <param name='point1'>The first Point to compare</param>
    /// <param name='point2'>The second Point to compare</param>
    public static bool operator ==(PointD point1, PointD point2)
    {
        return point1.X == point2.X &&
               point1.Y == point2.Y;
    }

    /// <summary>
    /// Compares two Point instances for exact inequality.
    /// Note that double values can acquire error when operated upon, such that
    /// an exact comparison between two values which are logically equal may fail.
    /// Furthermore, using this equality operator, Double.NaN is not equal to itself.
    /// </summary>
    /// <returns>
    /// bool - true if the two Point instances are exactly unequal, false otherwise
    /// </returns>
    /// <param name='point1'>The first Point to compare</param>
    /// <param name='point2'>The second Point to compare</param>
    public static bool operator !=(PointD point1, PointD point2)
    {
        return !(point1 == point2);
    }

    /// <summary>
    /// Compares two Point instances for object equality.  In this equality
    /// Double.NaN is equal to itself, unlike in numeric equality.
    /// Note that double values can acquire error when operated upon, such that
    /// an exact comparison between two values which
    /// are logically equal may fail.
    /// </summary>
    /// <returns>
    /// bool - true if the two Point instances are exactly equal, false otherwise
    /// </returns>
    /// <param name='point1'>The first Point to compare</param>
    /// <param name='point2'>The second Point to compare</param>
    public static bool Equals(PointD point1, PointD point2)
    {
        return point1.X.Equals(point2.X) &&
               point1.Y.Equals(point2.Y);
    }

    /// <summary>
    /// Equals - compares this Point with the passed in object.  In this equality
    /// Double.NaN is equal to itself, unlike in numeric equality.
    /// Note that double values can acquire error when operated upon, such that
    /// an exact comparison between two values which
    /// are logically equal may fail.
    /// </summary>
    /// <returns>
    /// bool - true if the object is an instance of Point and if it's equal to "this".
    /// </returns>
    /// <param name='o'>The object to compare to "this"</param>
    public override bool Equals(object? o)
    {
        if (o is null or not PointD)
        {
            return false;
        }

        PointD value = (PointD)o;
        return Equals(this, value);
    }

    /// <summary>
    /// Equals - compares this Point with the passed in object.  In this equality
    /// Double.NaN is equal to itself, unlike in numeric equality.
    /// Note that double values can acquire error when operated upon, such that
    /// an exact comparison between two values which
    /// are logically equal may fail.
    /// </summary>
    /// <returns>
    /// bool - true if "value" is equal to "this".
    /// </returns>
    /// <param name='value'>The Point to compare to "this"</param>
    public bool Equals(PointD value)
    {
        return Equals(this, value);
    }

    /// <summary>
    /// Returns the HashCode for this Point
    /// </summary>
    /// <returns>
    /// int - the HashCode for this Point
    /// </returns>
    public override int GetHashCode()
    {
        // Perform field-by-field XOR of HashCodes
        return X.GetHashCode() ^
               Y.GetHashCode();
    }

    #endregion Public Methods

    //------------------------------------------------------
    //
    //  Public Properties
    //
    //------------------------------------------------------

    #region Public Properties

    /// <summary>
    ///     X - double.  Default value is 0.
    /// </summary>
    public double X
    {
        get
        {
            return _x;
        }

        set
        {
            _x = value;
        }
    }

    /// <summary>
    ///     Y - double.  Default value is 0.
    /// </summary>
    public double Y
    {
        get
        {
            return _y;
        }

        set
        {
            _y = value;
        }
    }

    #endregion Public Properties

    //------------------------------------------------------
    //
    //  Protected Methods
    //
    //------------------------------------------------------

    #region Protected Methods

    #endregion ProtectedMethods

    //------------------------------------------------------
    //
    //  Internal Methods
    //
    //------------------------------------------------------

    #region Internal Methods

    #endregion Internal Methods

    //------------------------------------------------------
    //
    //  Internal Properties
    //
    //------------------------------------------------------

    #region Internal Properties

    /// <summary>
    /// Creates a string representation of this object based on the current culture.
    /// </summary>
    /// <returns>
    /// A string representation of this object.
    /// </returns>
    public override string ToString()
    {
        // Delegate to the internal method which implements all ToString calls.
        return ConvertToString(null /* format string */, null /* format provider */);
    }

    /// <summary>
    /// Creates a string representation of this object based on the IFormatProvider
    /// passed in.  If the provider is null, the CurrentCulture is used.
    /// </summary>
    /// <returns>
    /// A string representation of this object.
    /// </returns>
    public string ToString(IFormatProvider provider)
    {
        // Delegate to the internal method which implements all ToString calls.
        return ConvertToString(null /* format string */, provider);
    }

    /// <summary>
    /// Creates a string representation of this object based on the format string
    /// and IFormatProvider passed in.
    /// If the provider is null, the CurrentCulture is used.
    /// See the documentation for IFormattable for more information.
    /// </summary>
    /// <returns>
    /// A string representation of this object.
    /// </returns>
    string IFormattable.ToString(string format, IFormatProvider provider)
    {
        // Delegate to the internal method which implements all ToString calls.
        return ConvertToString(format, provider);
    }

    // Helper to get the numeric list separator for a given IFormatProvider.
    // Separator is a comma [,] if the decimal separator is not a comma, or a semicolon [;] otherwise.
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    static char GetNumericListSeparator(IFormatProvider provider)
    {
        char numericSeparator = ',';

        // Get the NumberFormatInfo out of the provider, if possible
        // If the IFormatProvider doesn't not contain a NumberFormatInfo, then
        // this method returns the current culture's NumberFormatInfo.
        NumberFormatInfo numberFormat = NumberFormatInfo.GetInstance(provider);

        Debug.Assert(null != numberFormat);

        // Is the decimal separator is the same as the list separator?
        // If so, we use the ";".
        if ((numberFormat.NumberDecimalSeparator.Length > 0) && (numericSeparator == numberFormat.NumberDecimalSeparator[0]))
        {
            numericSeparator = ';';
        }

        return numericSeparator;
    }

    /// <summary>
    /// Creates a string representation of this object based on the format string
    /// and IFormatProvider passed in.
    /// If the provider is null, the CurrentCulture is used.
    /// See the documentation for IFormattable for more information.
    /// </summary>
    /// <returns>
    /// A string representation of this object.
    /// </returns>
    internal string ConvertToString(string format, IFormatProvider provider)
    {
        // Helper to get the numeric list separator for a given culture.
        char separator = GetNumericListSeparator(provider);
        return string.Format(provider,
                             "{1:" + format + "}{0}{2:" + format + "}",
                             separator,
                             _x,
                             _y);
    }

    #endregion Internal Properties

    //------------------------------------------------------
    //
    //  Dependency Properties
    //
    //------------------------------------------------------

    #region Dependency Properties

    #endregion Dependency Properties

    //------------------------------------------------------
    //
    //  Internal Fields
    //
    //------------------------------------------------------

    #region Internal Fields

    internal double _x;
    internal double _y;

    #endregion Internal Fields

    #region Constructors

    //------------------------------------------------------
    //
    //  Constructors
    //
    //------------------------------------------------------

    #endregion Constructors
}