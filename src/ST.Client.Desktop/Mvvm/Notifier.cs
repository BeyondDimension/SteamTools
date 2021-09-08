using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace System.Application.Mvvm
{
    /// <summary>
    /// 属性更改通知支持
    /// </summary>
    public class Notifier : INotifyPropertyChanged
    {
        private event PropertyChangedEventHandler? PropertyChanged;

        event PropertyChangedEventHandler? INotifyPropertyChanged.PropertyChanged
        {
            add { PropertyChanged += value; }
            remove { PropertyChanged -= value; }
        }

        /// <summary>
        /// <see cref="INotifyPropertyChanged.PropertyChanged"/> 引发一个事件
        /// </summary>
        /// <param name="propertyName"></param>
        protected virtual void RaisePropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}