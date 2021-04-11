using System.Application.Entities;

namespace System.Application.Services
{
    public interface IAreaResource<out TArea> where TArea : class, IArea
    {
        /// <summary>
        /// 获取所有地区数据
        /// </summary>
        /// <returns></returns>
        TArea[] GetAll();

        /// <summary>
        /// 获取地区默认选择项
        /// </summary>
        TArea DefaultSelection { get; }
    }
}