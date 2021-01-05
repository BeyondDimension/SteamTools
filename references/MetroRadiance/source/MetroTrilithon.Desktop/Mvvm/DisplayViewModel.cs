using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Livet;
using MetroTrilithon.Serialization;

namespace MetroTrilithon.Mvvm
{
	public static class DisplayViewModel
	{
		public static DisplayViewModel<T> Create<T>(T value, string display)
		{
			return new DisplayViewModel<T> { Value = value, Display = display, };
		}

		public static DisplayViewModel<T> ToDefaultDisplay<T>(this SerializableProperty<T> property, string display)
		{
			return new DisplayViewModel<T> { Value = property.Default, Display = display, };
		}

		public static IEnumerable<DisplayViewModel<TResult>> ToDisplay<TSource, TResult>(this IEnumerable<TSource> source, Func<TSource, TResult> valueSelector, Func<TSource, string> displaySelector)
		{
			return source.Select(x => new DisplayViewModel<TResult> { Value = valueSelector(x), Display = displaySelector(x), });
		}
	}

	public class DisplayViewModel<T> : ViewModel
	{
		#region Value 変更通知プロパティ

		private T _Value;

		public T Value
		{
			get { return this._Value; }
			set
			{
				if (!Equals(this._Value, value))
				{
					this._Value = value;
					this.RaisePropertyChanged();
				}
			}
		}

		#endregion

		#region Display 変更通知プロパティ

		private string _Display;

		public string Display
		{
			get { return this._Display; }
			set
			{
				if (this._Display != value)
				{
					this._Display = value;
					this.RaisePropertyChanged();
				}
			}
		}

		#endregion

		public static implicit operator T(DisplayViewModel<T> dvm)
		{
			return dvm.Value;
		}

		public override string ToString()
		{
			return this.Display;
		}
	}
}
