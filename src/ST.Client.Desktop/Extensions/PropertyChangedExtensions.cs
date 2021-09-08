using StatefulModel.EventListeners;
using System.ComponentModel;

// ReSharper disable once CheckNamespace
namespace System
{
    public static class PropertyChangedExtensions
    {
        /// <summary>
        /// <see cref="INotifyPropertyChanged.PropertyChanged"/> 订阅事件
        /// </summary>
        public static IDisposable Subscribe(this INotifyPropertyChanged source, PropertyChangedEventHandler handler)
        {
            return new PropertyChangedEventListener(source, handler);
        }

        /// <summary>
        /// <see cref="INotifyPropertyChanged.PropertyChanged"/> 订阅事件
        /// </summary>
        /// <param name="source">イベント ソース。</param>
        /// <param name="action">イベント発生時に、<see cref="PropertyChangedEventArgs.PropertyName"/> を受け取って実行されるメソッド。</param>
        public static IDisposable Subscribe(this INotifyPropertyChanged source, Action<string> action)
        {
            return new PropertyChangedEventListener(source, (sender, args) => action(args.PropertyName));
        }

        /// <summary>
        /// 与指定的属性名称一起执行 <see cref="INotifyPropertyChanged.PropertyChanged"/> 订阅事件
        /// </summary>
        /// <param name="source">事件源</param>
        /// <param name="propertyName">订阅事件的属性的名称</param>
        /// <param name="action">事件发生时执行的方法</param>
        /// <param name="immediately">在调用此方法时 <paramref name="action"/> 如果只运行一次，则为true，否则为false，默认值是true。</param>
        public static ListenerWrapper Subscribe(this INotifyPropertyChanged source, string propertyName, Action action, bool immediately = true)
        {
            return new ListenerWrapper(source).Subscribe(propertyName, action, immediately);
        }

        public class ListenerWrapper : IDisposable
        {
            //用于一个事件源
            //当要订阅多个属性更改通知时
            // EventSource // <-INotifyPropertyChanged对象
            // .Subscribe（nameof（Property1），（）=> {...}）
            // .Subscribe（nameof（Property2），（）=> {...}）
            // .Subscribe（nameof（Property3），（）=> {...}）
            // .AddTo（this）;
            //仅出于编写目的而准备

            private readonly PropertyChangedEventListener _listener;

            internal ListenerWrapper(INotifyPropertyChanged source)
            {
                _listener = new PropertyChangedEventListener(source);
            }

            /// <summary>
            /// 与指定的属性名称一起执行 <see cref="INotifyPropertyChanged.PropertyChanged"/> 订阅事件
            /// </summary>
            /// <param name="propertyName">订阅事件的属性的名称</param>
            /// <param name="action">事件发生时执行的方法。</param>
            /// <param name="immediately">在调用此方法时 <paramref name="action"/>  如果只运行一次，则为true，否则为false，默认值是true。</param>
            public ListenerWrapper Subscribe(string propertyName, Action action, bool immediately = true)
            {
                if (immediately) action();
                _listener.Add(propertyName, (sender, args) => action());
                return this;
            }

            void IDisposable.Dispose() => _listener.Dispose();
        }
    }
}