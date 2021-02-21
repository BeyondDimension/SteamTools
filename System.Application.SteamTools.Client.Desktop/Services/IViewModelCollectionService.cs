using System.Application.UI.ViewModels;
using System.Collections.Generic;

namespace System.Application.Services
{
    /// <summary>
    /// 视图模型组服务
    /// </summary>
    public interface IViewModelCollectionService
    {
        /// <summary>
        /// 获取所有的视图模型
        /// </summary>
        IEnumerable<ViewModelBase> ViewModels { get; }

        /// <summary>
        /// 根据类型过滤获取视图模型
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        IEnumerable<T> GetViewModels<T>();
    }
}