using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using System.Threading;

namespace System.Application.Mvvm
{
    /// <summary>
    /// 属性更改通知事件
    /// </summary>
    [Serializable]
    public class NotificationObject : INotifyPropertyChanged
    {
        /// <summary>
        /// 属性更改通知事件
        /// </summary>
        [field: NonSerialized]
        public event PropertyChangedEventHandler? PropertyChanged;

        /// <summary>
        /// 引发属性更改通知事件
        /// </summary>
        /// <param name="source"></param>
        /// <param name="propertyExpression">() => lambda表达式属性</param>
        /// <exception cref="NotSupportedException">() => 指定了具有非属性格式的Lambda表达式</exception>
        [NotifyPropertyChangedInvocator]
        // ReSharper disable once UnusedParameter.Global
        protected virtual void RaisePropertyChanged<T>(ref T source, [NotNull] Expression<Func<T>> propertyExpression)
        {
            if (propertyExpression == null) throw new ArgumentNullException(nameof(propertyExpression));

            if (!(propertyExpression.Body is MemberExpression))
                throw new NotSupportedException("此方法仅允许使用 ()=> properties形式的lambda表达式");

            var memberExpression = (MemberExpression)propertyExpression.Body;
            RaisePropertyChanged(memberExpression.Member.Name);
        }

        /// <summary>
        /// 引发属性更改通知事件
        /// </summary>
        /// <param name="propertyName">属性名称</param>
        [NotifyPropertyChangedInvocator]
        protected virtual void RaisePropertyChanged([CallerMemberName][DisallowNull] string propertyName = "")
        {
            var threadSafeHandler = Interlocked.CompareExchange(ref PropertyChanged, null, null);
            threadSafeHandler?.Invoke(this, EventArgsFactory.GetPropertyChangedEventArgs(propertyName));
        }

        /// <summary>
        /// 如果该值与上一个值不同，则将其更改并引发属性更改通知事件。
        /// </summary>
        /// <typeparam name="T">属性泛型</typeparam>
        /// <param name="source">原始值</param>
        /// <param name="value">新值</param>
        /// <param name="relatedProperties">属性名称数组，当此属性更改时会触发PropertyChanged事件</param>
        /// <param name="propertyName">属性名称</param>
        /// <returns>值是否已更改</returns>
        [NotifyPropertyChangedInvocator]
        protected bool RaisePropertyChangedIfSet<T>(ref T source, T value, string[]? relatedProperties = null,
            [CallerMemberName] string propertyName = "")
        {
            // 如果值相同则什么都不做
            if (EqualityComparer<T>.Default.Equals(source, value))
                return false;

            source = value;
            RaisePropertyChanged(propertyName);
            if (relatedProperties == null) return true;

            foreach (var p in relatedProperties)
                RaisePropertyChanged(p);

            return true;
        }

        /// <summary>
        /// 如果该值与上一个值不同，则将其更改并引发属性更改通知事件。
        /// </summary>
        /// <typeparam name="T">属性泛型</typeparam>
        /// <param name="source">原始值</param>
        /// <param name="value">新值</param>
        /// <param name="relatedProperty">属性名称字符串，当此属性更改时会触发PropertyChanged事件</param>
        /// <param name="propertyName">属性名称</param>
        /// <returns>值是否已更改</returns>
        [NotifyPropertyChangedInvocator]
        protected bool RaisePropertyChangedIfSet<T>(ref T source, T value, string relatedProperty,
            [CallerMemberName] string propertyName = "")
        {
            return RaisePropertyChangedIfSet(ref source, value, new[] { relatedProperty }, propertyName);
        }
    }
}