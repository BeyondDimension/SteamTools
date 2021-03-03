using System.Application.UI.ViewModels;

namespace System.Application
{
    public static class ViewModelExtensions
    {
        /// <summary>
        /// <see cref="IDisposable"/> 将对象添加到<see cref="ViewModelBase"/> 的<see cref="ViewModelBase.CompositeDisposable"/>中
        /// </summary>
        public static T AddTo<T>(this T disposable, ViewModelBase vm) where T : IDisposable
        {
            if (vm == null)
            {
                disposable.Dispose();
                return disposable;
            }

            if (vm.CompositeDisposable == null)
            {
                throw new ArgumentNullException(nameof(ViewModelBase.CompositeDisposable));
            }

            vm.CompositeDisposable.Add(disposable);
            return disposable;
        }
    }
}
