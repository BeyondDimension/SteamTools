// ReSharper disable once CheckNamespace
namespace BD.WTTS.Repositories.Abstractions;

public interface IScriptRepository : IRepository<Script, int>
{
    /// <summary>
    /// 脚本可导入数量最大值
    /// </summary>
    const int MaxValue = 100;

    Task<bool> ExistsScript(string md5, string sha512);

    Task<Script> ExistsScriptName(string name);

    Task<IList<Script>> GetAllAsync();

    Task SaveScriptEnable(ScriptDTO item);
}