using StatefulModel;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace System.Application.Mvvm
{
    /// <summary>
    ///   提供用于一次处理多个IDisposable对象的功能
    /// </summary>
    public class CompositeDisposable : IDisposable, ICollection<IDisposable>
    {
        [NotNull] private readonly object _lockObject = new object();
        [NotNull] private readonly List<IDisposable> _targetLists;
        private bool _disposed;

        /// <summary>
        ///   构造函数
        /// </summary>
        public CompositeDisposable()
        {
            _targetLists = new List<IDisposable>();
        }

        /// <summary>
        ///     构造函数
        /// </summary>
        /// <param name="sourceDisposableList">IDisposable集合</param>
        public CompositeDisposable([NotNull] IEnumerable<IDisposable> sourceDisposableList)
        {
            if (sourceDisposableList == null) throw new ArgumentNullException(nameof(sourceDisposableList));

            _targetLists = new List<IDisposable>(sourceDisposableList);
        }

        /// <summary>
        ///    获取IDisposable集合的枚举数
        /// </summary>
        /// <returns>IDisposableコレクションの列挙子</returns>
        public IEnumerator<IDisposable> GetEnumerator()
        {
            ThrowExceptionIfDisposed();
            lock (_lockObject) { return ((IEnumerable<IDisposable>)_targetLists.ToArray()).GetEnumerator(); }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            ThrowExceptionIfDisposed();
            lock (_lockObject) { return ((IEnumerable<IDisposable>)_targetLists.ToArray()).GetEnumerator(); }
        }

        /// <summary>
        ///     在最后添加IDisposable
        /// </summary>
        /// <param name="item">要添加的IDisposable</param>
        public void Add(IDisposable item)
        {
            if (item == null) throw new ArgumentNullException(nameof(item));

            ThrowExceptionIfDisposed();
            lock (_lockObject) { _targetLists.Add(item); }
        }

        /// <summary>
        ///     删除所有元素
        /// </summary>
        public void Clear()
        {
            ThrowExceptionIfDisposed();
            lock (_lockObject) { _targetLists.Clear(); }
        }

        /// <summary>
        ///     确定此集合中是否包含元素。
        /// </summary>
        /// <param name="item">包含的元素</param>
        /// <returns>是否包含在集合中</returns>
        public bool Contains(IDisposable item)
        {
            if (item == null) throw new ArgumentNullException(nameof(item));

            ThrowExceptionIfDisposed();
            lock (_lockObject) { return _targetLists.Contains(item); }
        }

        /// <summary>
        ///     将整个内容复制到兼容的一维数组中。 复制操作从目标数组上的指定索引处开始。
        /// </summary>
        /// <param name="array">复制目标数组</param>
        /// <param name="arrayIndex">从目标数组中复制位置的索引</param>
        public void CopyTo(IDisposable[] array, int arrayIndex)
        {
            ThrowExceptionIfDisposed();
            lock (_lockObject) { _targetLists.CopyTo(array, arrayIndex); }
        }

        /// <summary>
        ///     获取实际存储的元素数
        /// </summary>
        public int Count
        {
            get
            {
                ThrowExceptionIfDisposed();
                lock (_lockObject) { return _targetLists.Count; }
            }
        }

        /// <summary>
        ///     获取此集合是否为只读 （总是返回false）
        /// </summary>
        public bool IsReadOnly
        {
            get
            {
                ThrowExceptionIfDisposed();
                return false;
            }
        }

        /// <summary>
        ///     删除找到的第一个特定对象
        /// </summary>
        /// <param name="item">要删除的对象</param>
        /// <returns>是否删除成功</returns>
        public bool Remove(IDisposable item)
        {
            if (item == null) throw new ArgumentNullException(nameof(item));

            ThrowExceptionIfDisposed();

            lock (_lockObject) { return _targetLists.Remove(item); }
        }

        /// <summary>
        ///     释放此集合中的所有元素
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        ///   代替IDisposable，添加一个Action来最后释放资源。
        /// </summary>
        /// <param name="releaseAction">使用Action释放资源</param>
        public void Add([NotNull] Action releaseAction)
        {
            if (releaseAction == null) throw new ArgumentNullException(nameof(releaseAction));

            ThrowExceptionIfDisposed();
            var disposable = new AnonymousDisposable(releaseAction);
            lock (_lockObject) { _targetLists.Add(disposable); }
        }

        /// <summary>
        ///     在开始处添加一个IDisposable
        /// </summary>
        /// <param name="item">要添加的IDisposable</param>
        public void AddFirst(IDisposable item)
        {
            if (item == null) throw new ArgumentNullException(nameof(item));

            ThrowExceptionIfDisposed();
            lock (_lockObject) { _targetLists.Insert(0, item); }
        }

        /// <summary>
        ///     代替IDisposable，添加在开始时释放资源的Action。
        /// </summary>
        /// <param name="releaseAction">使用Action释放资源</param>
        public void AddFirst([NotNull] Action releaseAction)
        {
            if (releaseAction == null) throw new ArgumentNullException(nameof(releaseAction));

            ThrowExceptionIfDisposed();
            var disposable = new AnonymousDisposable(releaseAction);
            lock (_lockObject) { _targetLists.Insert(0, disposable); }
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed) return;

            if (disposing)
                lock (_lockObject) { _targetLists.ForEach(item => item?.Dispose()); }

            _disposed = true;
        }

        protected void ThrowExceptionIfDisposed()
        {
            if (_disposed) throw new ObjectDisposedException("CompositeDisposable");
        }
    }
}
