// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

//

namespace System.Windows
{
    /// <summary>
    /// Point - Defaults to 0,0
    /// </summary>
    public partial struct Point
    {
        #region Constructors

        /// <summary>
        /// Constructor which accepts the X and Y values
        /// </summary>
        /// <param name="x">The value for the X coordinate of the new Point</param>
        /// <param name="y">The value for the Y coordinate of the new Point</param>
        public Point(double x, double y)
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

        #endregion Public Methods
    }
}