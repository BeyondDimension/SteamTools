using System.Application.Entities;
using System.Application.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using static System.Application.Services.ISecurityService;

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
        [Obsolete("use GetAllSourceAsync")]
        Task<List<IGAPAuthenticatorDTO>> GetAllAsync(string? secondaryPassword = null);

        /// <summary>
        /// 从本地获取所有源数据
        /// </summary>
        /// <returns></returns>
        Task<GameAccountPlatformAuthenticator[]> GetAllSourceAsync();

        /// <summary>
        /// 从本地获取第一条源数据
        /// </summary>
        /// <returns></returns>
        Task<GameAccountPlatformAuthenticator?> GetFirstOrDefaultSourceAsync();

        /// <summary>
        /// 源数据中是否含有本地加密的数据
        /// </summary>
        /// <param name="sources"></param>
        /// <returns></returns>
        bool HasLocal(IEnumerable<GameAccountPlatformAuthenticator> sources)
            => sources.Any() && sources.Any(x => !x.IsNotLocal);

        /// <inheritdoc cref="HasLocal(IEnumerable{GameAccountPlatformAuthenticator})"/>
        bool HasLocal(params GameAccountPlatformAuthenticator[] sources)
        {
            IEnumerable<GameAccountPlatformAuthenticator> sources_ = sources;
            return HasLocal(sources_);
        }

        /// <inheritdoc cref="HasLocal(IEnumerable{GameAccountPlatformAuthenticator})"/>
        Task<bool> HasLocalAsync();

        /// <summary>
        /// 源数据中是否含有需要二级密码的数据
        /// </summary>
        /// <param name="sources"></param>
        /// <returns></returns>
        bool HasSecondaryPassword(IEnumerable<GameAccountPlatformAuthenticator> sources)
            => sources.Any() && sources.Any(x => x.IsNeedSecondaryPassword);

        /// <inheritdoc cref="HasSecondaryPassword(IEnumerable{GameAccountPlatformAuthenticator})"/>
        bool HasSecondaryPassword(params GameAccountPlatformAuthenticator[] sources)
        {
            IEnumerable<GameAccountPlatformAuthenticator> sources_ = sources;
            return HasSecondaryPassword(sources_);
        }

        /// <inheritdoc cref="HasSecondaryPassword(IEnumerable{GameAccountPlatformAuthenticator})"/>
        Task<bool> HasSecondaryPasswordAsync();

        /// <summary>
        /// 将源数据转换为传输模型
        /// </summary>
        /// <param name="sources"></param>
        /// <param name="secondaryPassword"></param>
        /// <returns></returns>
        Task<List<IGAPAuthenticatorDTO>> ConvertToList(IEnumerable<GameAccountPlatformAuthenticator> sources, string? secondaryPassword = null);

        /// <summary>
        /// 插入或更新一条到本地，插入前需要判断当前值是否超过了 <see cref="MaxValue"/>
        /// </summary>
        /// <param name="item"></param>
        /// <param name="isLocal"></param>
        /// <param name="secondaryPassword"></param>
        /// <returns></returns>
        Task InsertOrUpdateAsync(IGAPAuthenticatorDTO item, bool isLocal, string? secondaryPassword = null);

        /// <inheritdoc cref="InsertOrUpdateAsync(IGAPAuthenticatorDTO, bool, string?)"/>
        async Task InsertOrUpdateAsync(IEnumerable<IGAPAuthenticatorDTO> items, bool isLocal, string? secondaryPassword = null)
        {
            foreach (var item in items)
            {
                await InsertOrUpdateAsync(item, isLocal, secondaryPassword);
            }
        }

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
        /// 根据本地Id获取源
        /// </summary>
        /// <param name="id"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        ValueTask<GameAccountPlatformAuthenticator?> FindAsync(ushort id, CancellationToken cancellationToken = default);

        /// <summary>
        /// 根据本地源修改显示名称
        /// </summary>
        /// <param name="source"></param>
        /// <param name="name"></param>
        /// <param name="isLocal"></param>
        /// <returns></returns>
        Task RenameAsync(GameAccountPlatformAuthenticator source, string name, bool isLocal);

        /// <summary>
        /// 根据本地Id修改显示名称
        /// </summary>
        /// <param name="id"></param>
        /// <param name="name"></param>
        /// <param name="isLocal"></param>
        /// <returns></returns>
        Task RenameAsync(ushort id, string name, bool isLocal);

        /// <summary>
        /// 根据本地Id设置云同步Id
        /// </summary>
        /// <param name="id"></param>
        /// <param name="serverId"></param>
        /// <returns></returns>
        Task SetServerIdAsync(ushort id, Guid serverId);

        /// <inheritdoc cref="SetServerIdAsync(ushort, Guid)"/>
        Task SetServerIdAsync(GameAccountPlatformAuthenticator source, Guid serverId);

        /// <summary>
        /// 切换加密模式，对已有的整个数据重新加密，调用前必须判断值不一样才能调用，如果耗时太长要搞一个转圈或进度条
        /// </summary>
        /// <param name="isLocal"></param>
        /// <param name="secondaryPassword"></param>
        /// <param name="items">可传递当前在视图模型上的数据，也可以传递 <see langword="null"/> 重新从数据库中查询</param>
        /// <returns></returns>
        Task SwitchEncryptionModeAsync(bool isLocal, string? secondaryPassword, IEnumerable<IGAPAuthenticatorDTO>? items = null);

        /// <summary>
        /// 导出一组DTO模型
        /// </summary>
        /// <param name="isLocal"></param>
        /// <param name="secondaryPassword"></param>
        /// <param name="items"></param>
        /// <returns></returns>
        Task<byte[]> ExportAsync(bool isLocal, string? secondaryPassword, IEnumerable<IGAPAuthenticatorDTO> items);

        /// <inheritdoc cref="ExportAsync(bool, string?, IEnumerable{IGAPAuthenticatorDTO})"/>
        Task<byte[]> ExportAsync(bool isLocal, string? secondaryPassword, params IGAPAuthenticatorDTO[] items)
        {
            IEnumerable<IGAPAuthenticatorDTO> sources_ = items;
            return ExportAsync(isLocal, secondaryPassword, sources_);
        }

        /// <summary>
        /// 导入一组数据，返回对应的DTO模型组，之后可调用 <see cref="InsertOrUpdateAsync(IEnumerable{IGAPAuthenticatorDTO}, bool, string?)"/> 插入数据库
        /// </summary>
        /// <param name="secondaryPassword"></param>
        /// <param name="content"></param>
        /// <returns></returns>
        Task<(ImportResultCode resultCode, IReadOnlyList<IGAPAuthenticatorDTO> result, int sourcesCount)> ImportAsync(string? secondaryPassword, byte[] content);

        /// <summary>
        /// 移动排序值，上移或下移，返回受影响的行数
        /// </summary>
        /// <param name="items"></param>
        /// <param name="index"></param>
        /// <param name="upOrDown"></param>
        /// <returns></returns>
        Task<int> MoveOrderByIndexAsync(IList<IGAPAuthenticatorDTO> items, int index, bool upOrDown);

        /// <inheritdoc cref="MoveOrderByIndexAsync(IList{IGAPAuthenticatorDTO}, int, bool)"/>
        async Task<int> MoveOrderByItemAsync(IList<IGAPAuthenticatorDTO> items, IGAPAuthenticatorDTO item, bool upOrDown)
        {
            var index = items.IndexOf(item);
            if (index > -1)
            {
                return await MoveOrderByIndexAsync(items, index, upOrDown);
            }
            return 0;
        }

        /// <inheritdoc cref="MoveOrderByIndexAsync(IList{IGAPAuthenticatorDTO}, int, bool)"/>
        async Task<int> MoveOrderByIdAsync(IList<IGAPAuthenticatorDTO> items, int id, bool upOrDown)
        {
            var index = -1;
            for (int i = 0; i < items.Count; i++)
            {
                var item = items[i];
                if (item.Id == id)
                {
                    index = i;
                    break;
                }
            }
            if (index > -1)
            {
                return await MoveOrderByIndexAsync(items, index, upOrDown);
            }
            return 0;
        }

        public enum ImportResultCode
        {
            /// <summary>
            /// 导入成功
            /// </summary>
            Success = 200,

            /// <summary>
            /// 部分数据导入成功
            /// </summary>
            PartSuccess,

            /// <summary>
            /// 格式不正确，反序列化失败
            /// </summary>
            IncorrectFormat = 400,

            /// <inheritdoc cref="DResultCode.EmbeddedAesFail"/>
            EmbeddedAesFail = DResultCode.EmbeddedAesFail,

            /// <inheritdoc cref="DResultCode.LocalFail"/>
            LocalFail = DResultCode.LocalFail,

            /// <inheritdoc cref="DResultCode.SecondaryPasswordFail"/>
            SecondaryPasswordFail = DResultCode.SecondaryPasswordFail,

            /// <inheritdoc cref="DResultCode.IncorrectValueFail"/>
            IncorrectValueFail = DResultCode.IncorrectValueFail,

            /// <inheritdoc cref="DResultCode.UTF8GetStringFail"/>
            UTF8GetStringFail = DResultCode.UTF8GetStringFail,
        }
    }
}