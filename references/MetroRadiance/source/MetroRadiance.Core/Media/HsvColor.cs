using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Media;

namespace MetroRadiance.Media
{
	/// <summary>
	/// 色相、彩度および明度の各成分による HSV 色空間で色を表します。
	/// </summary>
	[Serializable]
	public struct HsvColor : IEquatable<HsvColor>
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
		/// 0.0 から 1.0 の範囲で表される明度の値を取得または設定します。
		/// </summary>
		public double V { get; set; }

		public bool Equals(HsvColor color)
		{
			return this == color;
		}

		public override bool Equals(object obj)
		{
			if (ReferenceEquals(null, obj)) return false;
			return obj is HsvColor && this.Equals((HsvColor)obj);
		}

		public override int GetHashCode()
		{
			unchecked
			{
				// ReSharper disable NonReadonlyMemberInGetHashCode
				var hashCode = this.H.GetHashCode();
				hashCode = (hashCode * 397) ^ this.S.GetHashCode();
				hashCode = (hashCode * 397) ^ this.V.GetHashCode();
				// ReSharper restore NonReadonlyMemberInGetHashCode
				return hashCode;
			}
		}

		public override string ToString()
		{
			return $"H:{this.H:##.000}, S:{(int)(this.S * 100)}, V:{(int)(this.V * 100)}";
		}

		public static bool operator ==(HsvColor color1, HsvColor color2)
		{
			return color1.H.Equals(color2.H)
				&& color1.S.Equals(color2.S)
				&& color1.V.Equals(color2.V);
		}

		public static bool operator !=(HsvColor color1, HsvColor color2)
		{
			return !(color1 == color2);
		}

		/// <summary>
		/// 現在の HSV 色空間による色から RGB 色空間の <see cref="Color"/> 構造体を作成します。
		/// </summary>
		public Color ToRgb()
		{
			var hi = (int)(this.H / 60.0) % 6;
			var f = this.H / 60.0f - (int)(this.H / 60.0);
			var p = this.V * (1 - this.S);
			var q = this.V * (1 - f * this.S);
			var t = this.V * (1 - (1 - f) * this.S);

			double r, g, b;

			switch (hi)
			{
				case 0:
					r = this.V;
					g = t;
					b = p;
					break;

				case 1:
					r = q;
					g = this.V;
					b = t;
					break;

				case 2:
					r = p;
					g = this.V;
					b = t;
					break;

				case 3:
					r = p;
					g = q;
					b = this.V;
					break;

				case 4:
					r = t;
					g = p;
					b = this.V;
					break;

				case 5:
					r = this.V;
					g = p;
					b = q;
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
		/// 指定した色相、彩度および明度の値を使用して新しい <see cref="HsvColor"/> 構造体を作成します。
		/// </summary>
		/// <param name="h">新しい色の 0 から 260 の範囲で表される色相。</param>
		/// <param name="s">新しい色の 0.0 から 1.0 の範囲で表される彩度。</param>
		/// <param name="v">新しい色の 0.0 から 1.0 の範囲で表される明度。</param>
		public static HsvColor FromHsv(double h, double s, double v)
		{
			return new HsvColor
			{
				H = h,
				S = s,
				V = v,
			};
		}

		/// <summary>
		/// 指定した RGB 色空間の色を使用して新しい <see cref="HsvColor"/> 構造体を作成します。
		/// </summary>
		public static HsvColor FromRgb(Color color)
		{
			return FromRgb(color.R, color.G, color.B);
		}

		/// <summary>
		/// 指定した RGB 色空間の色を使用して新しい <see cref="HsvColor"/> 構造体を作成します。
		/// </summary>
		public static HsvColor FromRgb(byte r, byte g, byte b)
		{
			var max = Math.Max(Math.Max(r, g), b);
			var min = Math.Min(Math.Min(r, g), b);

			double h, s;
			if (max == min)
			{
				h = 0;
			}
			else if (max == r)
			{
				h = (60 * (g - b) / ((double)max - min) + 360) % 360;
			}
			else if (max == g)
			{
				h = (60 * (b - r) / ((double)max - min)) + 120;
			}
			else if (max == b)
			{
				h = (60 * (r - g) / ((double)max - min)) + 240;
			}
			else
			{
				throw new ArgumentException();
			}

			if (max == 0)
			{
				s = 0;
			}
			else
			{
				s = ((max - min) / (double)max);
			}

			var v = max / 255.0;
			return FromHsv(h, s, v);
		}
	}
}
