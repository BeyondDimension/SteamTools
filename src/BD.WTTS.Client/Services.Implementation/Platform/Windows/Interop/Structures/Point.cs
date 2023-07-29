// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.
// https://github.com/dotnet/wpf/blob/v6.0.0-rc.2.21501.7/src/Microsoft.DotNet.Wpf/src/WindowsBase/System/Windows/Point.cs

using System.Drawing;
using PointInt32 = System.Drawing.Point;

// ReSharper disable once CheckNamespace
namespace System.Windows;

/// <summary>
/// Point - Defaults to 0,0
/// </summary>
public partial struct PointD
{
    #region Constructors

    /// <summary>
    /// Constructor which accepts the X and Y values
    /// </summary>
    /// <param name="x">The value for the X coordinate of the new Point</param>
    /// <param name="y">The value for the Y coordinate of the new Point</param>
    public PointD(double x, double y)
    {
        _x = x;
        _y = y;
    }

    #endregion Constructors

    #region Public Methods

    /// <summary>
    /// Offset - update the location by adding offsetX to X and offsetY to Y
    /// </summary>
    /// <param name="offsetX"> The offset in the x dimension </param>
    /// <param name="offsetY"> The offset in the y dimension </param>
    public void Offset(double offsetX, double offsetY)
    {
        _x += offsetX;
        _y += offsetY;
    }

    /// <summary>
    /// Explicit conversion to Size.  Note that since Size cannot contain negative values,
    /// the resulting size will contains the absolute values of X and Y
    /// </summary>
    /// <returns>
    /// Size - A Size equal to this Point
    /// </returns>
    /// <param name="point"> Point - the Point to convert to a Size </param>
    public static explicit operator Size(PointD point)
    {
        return new Size(Convert.ToInt32(Math.Abs(point._x)), Convert.ToInt32(Math.Abs(point._y)));
    }

    public static explicit operator PointInt32(PointD point)
    {
        return new PointInt32(Convert.ToInt32(point._x), Convert.ToInt32(point._y));
    }

    #endregion Public Methods
}