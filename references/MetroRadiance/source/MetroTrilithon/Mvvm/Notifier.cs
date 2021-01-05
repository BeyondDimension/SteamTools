using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace MetroTrilithon.Mvvm
{
	/// <summary>
	/// プロパティ変更通知をサポートします。
	/// </summary>
	public class Notifier : INotifyPropertyChanged
	{
		private event PropertyChangedEventHandler _propertyChanged;

		event PropertyChangedEventHandler INotifyPropertyChanged.PropertyChanged
		{
			add { this._propertyChanged += value; }
			remove { this._propertyChanged -= value; }
		}

		/// <summary>
		/// <see cref="INotifyPropertyChanged.PropertyChanged"/> イベントを発生させます。
		/// </summary>
		/// <param name="propertyName"></param>
		protected virtual void RaisePropertyChanged([CallerMemberName] string propertyName = null)
		{
			this._propertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}
	}
}
