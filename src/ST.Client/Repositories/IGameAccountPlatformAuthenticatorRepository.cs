using System.Application.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace System.Application.Repositories
{
    public interface IGameAccountPlatformAuthenticatorRepository
    {
        /// <summary>
        /// 游戏账号平台令牌可导入数量最大值
        /// </summary>
        public const int MaxValue = 1000;

        /// <summary>
        /// 从本地获取所有
        /// </summary>
        /// <param name="secondaryPassword"></param>
        /// <returns></returns>
        Task<List<IGAPAuthenticatorDTO>> GetAllAsync(string? secondaryPassword = null);

        /// <summary>
        /// 插入或更新一条到本地，插入前需要判断当前值是否超过了 <see cref="MaxValue"/>
        /// </summary>
        /// <param name="item"></param>
        /// <param name="isLocal"></param>
        /// <param name="secondaryPassword"></param>
        /// <returns></returns>
        Task InsertOrUpdateAsync(IGAPAuthenticatorDTO item, bool isLocal, string? secondaryPassword = null);

        /// <summary>
        /// 根据本地Id删除一条
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        Task DeleteAsync(ushort id);

        /// <summary>
        /// 根据云同步Id删除
        /// </summary>
        /// <param name="serverId"></param>
        /// <returns></returns>
        Task DeleteAsync(Guid serverId);

        /// <summary>
        /// 根据本地Id修改显示名称
        /// </summary>
        /// <param name="id"></param>
        /// <param name="name"></param>
        /// <param name="isLocal"></param>
        /// <param name="secondaryPassword"></param>
        /// <returns></returns>
        Task RenameAsync(ushort id, string name, bool isLocal, string? secondaryPassword = null);

        /// <summary>
        /// 根据本地Id设置云同步Id
        /// </summary>
        /// <param name="id"></param>
        /// <param name="serverId"></param>
        /// <returns></returns>
        Task SetServerIdAsync(ushort id, Guid serverId);

        /// <summary>
        /// 切换加密模式，对已有的整个数据重新加密，调用前必须判断值不一样才能调用，如果耗时太长要搞一个转圈或进度条
        /// </summary>
        /// <param name="isLocal"></param>
        /// <param name="secondaryPassword"></param>
        /// <param name="items">可传递当前在视图模型上的数据，也可以传递 <see langword="null"/> 重新从数据库中查询</param>
        /// <returns></returns>
        Task SwitchEncryptionModeAsync(bool isLocal, string? secondaryPassword, IEnumerable<IGAPAuthenticatorDTO>? items = null);
    }
}