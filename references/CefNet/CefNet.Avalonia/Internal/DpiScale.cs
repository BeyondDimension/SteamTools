using System;
using System.Collections.Generic;
using System.Text;
using Avalonia;

namespace CefNet.Internal
{
	/// <summary>
	/// Stores DPI information from which a <see cref="Avalonia.Layout.Visual"/> is rendered.
	/// </summary>
	/// 
	public struct DpiScale
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="DpiScale"/> structure.
		/// </summary>
		/// <param name="dpiScaleX">The DPI scale on the X axis.</param>
		/// <param name="dpiScaleY">The DPI scale on the Y axis.</param>
		public DpiScale(double dpiScaleX, double dpiScaleY)
		{
			this.DpiScaleX = dpiScaleX;
			this.DpiScaleY = dpiScaleY;
			this.PixelsPerDip = 96.0;
			this.PixelsPerInchX = dpiScaleX * 96.0;
			this.PixelsPerInchY = dpiScaleY * 96.0;
		}

		/// <summary>
		/// Gets the DPI scale on the X axis.
		/// </summary>
		public double DpiScaleX { get; }

		/// <summary>
		/// Gets the DPI scale on the Y axis.
		/// </summary>
		public double DpiScaleY { get; }

		/// <summary>
		/// Gets the PixelsPerDip at which the text should be rendered.
		/// </summary>
		public double PixelsPerDip { get; }

		/// <summary>
		/// Gets the DPI along X axis.
		/// </summary>
		public double PixelsPerInchX { get; }

		/// <summary>
		/// Gets the DPI along Y axis.
		/// </summary>
		public double PixelsPerInchY { get; }

		/// <summary>
		/// Gets the DPI.
		/// </summary>
		public Vector Dpi
		{
			get { return new Vector(PixelsPerInchX, PixelsPerInchY); }
		}
	}
}
