using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace MetroRadiance.UI.Controls
{
	/// <summary>
	/// 入力された値が有効な <see cref="Int32"/> 値かどうかを検証します。
	/// </summary>
	public class Int32Rule : ValidationRule
	{
		/// <summary>
		/// 入力に空文字を許可するかどうかを示す値を取得または設定します。
		/// </summary>
		public bool AllowsEmpty { get; set; }

		/// <summary>
		/// 入力可能な最小値を取得または設定します。
		/// </summary>
		/// <value>
		/// 入力可能な最小値。最小値がない場合は null。
		/// </value>
		public int? Min { get; set; }

		/// <summary>
		/// 入力可能な最大値を取得または設定します。
		/// </summary>
		/// <value>
		/// 入力可能な最大値。最大値がない場合は null。
		/// </value>
		public int? Max { get; set; }


		public override ValidationResult Validate(object value, CultureInfo cultureInfo)
		{
			var numberAsString = value as string;
			if (string.IsNullOrEmpty(numberAsString))
			{
				return this.AllowsEmpty
					? new ValidationResult(true, null)
					: new ValidationResult(false, "値を入力してください。");
			}

			int number;
			try
			{
				number = int.Parse(numberAsString);
			}
			catch (Exception)
			{
				return new ValidationResult(false, "数値を入力してください。");
			}

			if (this.Min.HasValue && number < this.Min)
			{
				return new ValidationResult(false, $"{this.Min} 以上の数値を入力してください。");
			}

			if (this.Max.HasValue && this.Max < number)
			{
				return new ValidationResult(false, $"{this.Max} 以下の数値を入力してください。");
			}

			return new ValidationResult(true, null);
		}
	}
}
