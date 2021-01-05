namespace MetroRadiance.Media
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Runtime.CompilerServices;
	using System.Text;
	using System.Threading.Tasks;
	using System.Windows.Media;

	[Serializable]
	public struct HslColor : IEquatable<HslColor>
	{
		/// <summary>
		/// 0 から 360 の範囲で表される色相の値を取得または設定します。
		/// </summary>
		public double H { get; set; }

		/// <summary>
		/// 0.0 から 1.0 の範囲で表される彩度の値を取得または設定します。
		/// </summary>
		public double S { get; set; }

		/// <summary>
		/// 0.0 から 1.0 の範囲で表される輝度の値を取得または設定します。
		/// </summary>
		public double L { get; set; }

		public bool Equals(HslColor color)
		{
			return this == color;
		}

		public override bool Equals(object obj)
		{
			if (ReferenceEquals(null, obj)) return false;
			return obj is HslColor && this.Equals((HslColor)obj);
		}

		public override int GetHashCode()
		{
			unchecked
			{
				// ReSharper disable NonReadonlyMemberInGetHashCode
				var hashCode = this.H.GetHashCode();
				hashCode = (hashCode * 397) ^ this.S.GetHashCode();
				hashCode = (hashCode * 397) ^ this.L.GetHashCode();
				// ReSharper restore NonReadonlyMemberInGetHashCode
				return hashCode;
			}
		}

		public override string ToString()
		{
			return $"H:{this.H:##.000}, S:{(int)(this.S * 100)}, L:{(int)(this.L * 100)}";
		}

		public static bool operator ==(HslColor color1, HslColor color2)
		{
			return color1.H.Equals(color2.H)
				&& color1.S.Equals(color2.S)
				&& color1.L.Equals(color2.L);
		}

		public static bool operator !=(HslColor color1, HslColor color2)
		{
			return !(color1 == color2);
		}

		/// <summary>
		/// 現在の HSL 色空間による色から RGB 色空間の <see cref="Color"/> 構造体を作成します。
		/// </summary>
		public Color ToRgb()
		{
			var max = this.L + (this.S * (1 - Math.Abs(2 * this.L - 1))) / 2;
			var min = this.L - (this.S * (1 - Math.Abs(2 * this.L - 1))) / 2;
			var hd = this.H % 360.0;
			if (hd < 0) hd += 360.0;
			var hi = (int)(hd / 60.0) % 6;

			double r, g, b;
			switch (hi)
			{
				case 0:
					r = max;
					g = min + (max - min) * hd / 60;
					b = min;
					break;

				case 1:
					r = min + (max - min) * (120 - hd) / 60;
					g = max;
					b = min;
					break;

				case 2:
					r = min;
					g = max;
					b = min + (max - min) * (hd - 120) / 60;
					break;

				case 3:
					r = min;
					g = min + (max - min) * (240 - hd) / 60;
					b = max;
					break;

				case 4:
					r = min + (max - min) * (hd - 240) / 60;
					g = min;
					b = max;
					break;

				case 5:
					r = max;
					g = min;
					b = min + (max - min) * (360 - hd) / 60;
					break;

				default:
					throw new InvalidOperationException();
			}

			return Color.FromRgb(
				(byte)Math.Max(Math.Min(r * 255, 255), .0),
				(byte)Math.Max(Math.Min(g * 255, 255), .0),
				(byte)Math.Max(Math.Min(b * 255, 255), .0));
		}

		/// <summary>
		/// 指定した色相、彩度および輝度の値を使用して新しい <see cref="HslColor"/> 構造体を作成します。
		/// </summary>
		/// <param name="h">新しい色の 0 から 360 の範囲で表される色相。</param>
		/// <param name="s">新しい色の 0.0 から 1.0 の範囲で表される彩度。</param>
		/// <param name="l">新しい色の 0.0 から 1.0 の範囲で表される輝度。</param>
		public static HslColor FromHsl(double h, double s, double l)
		{
			return new HslColor
			{
				H = h,
				S = s,
				L = l,
			};
		}

		/// <summary>
		/// 指定した RGB 色空間の色を使用して新しい <see cref="HslColor"/> 構造体を作成します。
		/// </summary>
		public static HslColor FromRgb(Color color)
		{
			return FromRgb(color.R, color.G, color.B);
		}

		/// <summary>
		/// 指定した RGB 色空間の色を使用して新しい <see cref="HsvColor"/> 構造体を作成します。
		/// </summary>
		public static HslColor FromRgb(byte r, byte g, byte b)
		{
			var max = Math.Max(Math.Max(r, g), b);
			var min = Math.Min(Math.Min(r, g), b);

			double h, s;
			if (max == min)
			{
				h = 0;
			}
			else if (min == b)
			{
				h = ((60 * (g - r) / ((double)max - min) + 60)) % 360;
			}
			else if (min == r)
			{
				h = ((60 * (b - g) / ((double)max - min)) + 180) % 360;
			}
			else if (min == g)
			{
				h = ((60 * (r - b) / ((double)max - min)) + 300) % 360;
			}
			else
			{
				throw new ArgumentException();
			}
			var l = ((double)max + min) / 255.0 / 2;

			if (max == 0)
			{
				s = 0;
			}
			else
			{
				s = (max - min) / (255.0 - Math.Abs((double)max + min - 255));
			}

			return FromHsl(h, s, l);
		}
	}
}
