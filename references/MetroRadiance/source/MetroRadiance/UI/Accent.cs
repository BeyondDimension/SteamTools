using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows.Media;

namespace MetroRadiance.UI
{
	/// <summary>
	/// MetroRadiance テーマのアクセントとして使用する色を識別します。
	/// </summary>
	[Serializable]
	public struct Accent : IEquatable<Accent>
	{
		// Blue、Orange、Purple は Specified で指定
		// 色を設定する場合は Color で指定
		// Specified、Color が両方 null だった場合は Windows テーマ追従
		// 既定値で構造体が作られると両方 null なので、new Accent(); すると Windows テーマ追従になる

		/// <summary>
		/// アクセント専用の色が指定されている場合、その色を示す識別子を取得します。
		/// </summary>
		public SpecifiedColor? Specified { get; private set; }

		/// <summary>
		/// アクセントとして使用する色が指定されている場合、その色を示す <see cref="Color"/> 構造体を取得します。
		/// </summary>
		public Color? Color { get; private set; }

		/// <summary>
		/// Windows の色と同期するかどうかを示す値を取得します。
		/// </summary>
		public bool SyncToWindows => this.Specified == null && this.Color == null;

		private Accent(SpecifiedColor specified)
		{
			this.Specified = specified;
			this.Color = null;
		}

		private Accent(Color color)
		{
			this.Color = color;
			this.Specified = null;
		}

		public bool Equals(Accent accent)
		{
			return this == accent;
		}

		public override bool Equals(object obj)
		{
			if (ReferenceEquals(null, obj)) return false;
			return obj is Accent && this.Equals((Accent)obj);
		}

		public override int GetHashCode()
		{
			unchecked
			{
				// ReSharper disable NonReadonlyMemberInGetHashCode
				return (this.Specified.GetHashCode() * 397) ^ this.Color.GetHashCode();
				// ReSharper restore NonReadonlyMemberInGetHashCode
			}
		}

		public override string ToString()
		{
			if (this.Specified != null) return this.Specified.Value.ToString();
			if (this.Color != null) return this.Color.Value.ToString();

			return "(Sync to Windows)";
		}

		public static bool operator ==(Accent accent1, Accent accent2)
		{
			return accent1.Specified == accent2.Specified
				   && accent1.Color == accent2.Color;
		}

		public static bool operator !=(Accent accent1, Accent accent2)
		{
			return !(accent1 == accent2);
		}

		public static Accent Blue { get; } = new Accent(SpecifiedColor.Blue);
		public static Accent Orange { get; } = new Accent(SpecifiedColor.Orange);
		public static Accent Purple { get; } = new Accent(SpecifiedColor.Purple);

		/// <summary>
		/// Windows のテーマ設定に追従するアクセントを取得します。
		/// </summary>
		public static Accent Windows { get; } = new Accent();

		/// <summary>
		/// 指定した <see cref="Color"/> 構造体を使用して、その色のアクセントを作成します。
		/// </summary>
		/// <param name="color">アクセントとして使用する色を示す <see cref="Color"/> 構造体。</param>
		public static Accent FromColor(Color color) => new Accent(color);

		public enum SpecifiedColor
		{
			Purple,
			Blue,
			Orange,
		}

		/// <summary>
		/// シリアル化インフラストラクチャをサポートします。コードから直接使用するためのものではありません。
		/// </summary>
		[EditorBrowsable(EditorBrowsableState.Never)]
		// ReSharper disable once InconsistentNaming
		public SpecifiedColor? __Specified
		{
			get { return this.Specified; }
			set { this.Specified = value; }
		}

		/// <summary>
		/// シリアル化インフラストラクチャをサポートします。コードから直接使用するためのものではありません。
		/// </summary>
		[EditorBrowsable(EditorBrowsableState.Never)]
		// ReSharper disable once InconsistentNaming
		public Color? __Color
		{
			get { return this.Color; }
			set { this.Color = value; }
		}
	}
}
