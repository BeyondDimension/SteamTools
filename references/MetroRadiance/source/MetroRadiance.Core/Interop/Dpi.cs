using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace MetroRadiance.Interop
{
	/// <summary>
	/// モニターの DPI (dots per inch) を表します。
	/// </summary>
	[DebuggerDisplay("X = {X} ({ScaleX}), Y = {Y} ({ScaleY})")]
	public struct Dpi : IEquatable<Dpi>
	{
		public static readonly Dpi Default = new Dpi(96, 96);

		private double? scaleX;
		private double? scaleY;

		public uint X { get; }
		public uint Y { get; }

		public double ScaleX => this.scaleX ?? (this.scaleX = this.X / (double)Default.X).Value;

		public double ScaleY => this.scaleY ?? (this.scaleY = this.Y / (double)Default.Y).Value;

		public Dpi(uint x, uint y)
			: this()
		{
			this.X = x;
			this.Y = y;
		}

		public Point LogicalToPhysical(Point logical)
			=> new Point(logical.X * this.ScaleX, logical.Y * this.ScaleY);

		public Point PhysicalToLogical(Point physical)
			=> new Point(physical.X / this.ScaleX, physical.Y / this.ScaleY);

		public Size LogicalToPhysical(Size logical)
			=> new Size(logical.Width * this.ScaleX, logical.Height * this.ScaleY);

		public Size PhysicalToLogical(Size physical)
			=> new Size(physical.Width / this.ScaleX, physical.Height / this.ScaleY);

		public Rect LogicalToPhysical(Rect logical)
			=> new Rect(
				logical.X * this.ScaleX,
				logical.Y * this.ScaleY,
				logical.Width * this.ScaleX,
				logical.Height * this.ScaleY);

		public Rect PhysicalToLogical(Rect physical)
			=> new Rect(
				physical.X / this.ScaleX,
				physical.Y / this.ScaleY,
				physical.Width / this.ScaleX,
				physical.Height / this.ScaleY);

		public static bool operator ==(Dpi dpi1, Dpi dpi2)
		{
			return dpi1.X == dpi2.X && dpi1.Y == dpi2.Y;
		}

		public static bool operator !=(Dpi dpi1, Dpi dpi2)
		{
			return !(dpi1 == dpi2);
		}

		public bool Equals(Dpi other)
		{
			return this.X == other.X && this.Y == other.Y;
		}

		public override bool Equals(object obj)
		{
			if (ReferenceEquals(null, obj)) return false;
			return obj is Dpi && this.Equals((Dpi)obj);
		}

		public override int GetHashCode()
		{
			unchecked
			{
				return ((int)this.X * 397) ^ (int)this.Y;
			}
		}

		public static Dpi FromVisual(Visual visual)
		{
			var source = PresentationSource.FromVisual(visual);
			if (source == null) throw new ObjectDisposedException(nameof(visual));

			return FromPresentationSource(source);
		}

		public static Dpi FromPresentationSource(PresentationSource source)
			=> FromMatrix(source.CompositionTarget.TransformToDevice);

		public static Dpi FromMatrix(Matrix matrix)
			=> new Dpi((uint)(96 * matrix.M11), (uint)(96 * matrix.M22));
	}
}
