using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace System.Application.Mvvm
{
	/// <summary>
	/// 属性更改通知支持
	/// </summary>
	public class Notifier : INotifyPropertyChanged
	{
        private event PropertyChangedEventHandler PropertyChanged;

		event PropertyChangedEventHandler INotifyPropertyChanged.PropertyChanged
		{
			add { this.PropertyChanged += value; }
			remove { this.PropertyChanged -= value; }
		}

		/// <summary>
		/// <see cref="INotifyPropertyChanged.PropertyChanged"/> 引发一个事件
		/// </summary>
		/// <param name="propertyName"></param>
		protected virtual void RaisePropertyChanged([CallerMemberName] string propertyName = null)
		{
			this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}
	}
}
