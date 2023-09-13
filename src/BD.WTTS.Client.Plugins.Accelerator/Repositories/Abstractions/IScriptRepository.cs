// ReSharper disable once CheckNamespace
namespace BD.WTTS.Repositories.Abstractions;

public interface IScriptRepository : IRepository<Script, int>
{
    /// <summary>
    /// 脚本可导入数量最大值
    /// </summary>
    const int MaxValue = 100;

    Task<bool> ExistsScriptAsync(string md5, string sha512, CancellationToken cancellationToken = default);

    Task<Script> ExistsScriptNameAsync(string name, CancellationToken cancellationToken = default);

    Task<IList<Script>> GetAllAsync(CancellationToken cancellationToken = default);

    Task SaveScriptEnableAsync(ScriptDTO item, CancellationToken cancellationToken = default);

    Task SaveScriptCachePathAsync(ScriptDTO item, CancellationToken cancellationToken);
}