// ReSharper disable once CheckNamespace
namespace BD.WTTS.Services;

public interface IScriptManager
{
    const string DirName = "Scripts";

    static IScriptManager Instance => Ioc.Get<IScriptManager>();

    /// <summary>
    /// 加载列表 JS 内容
    /// </summary>
    /// <param name="all">加载的列表</param>
    /// <returns></returns>
    Task<IEnumerable<ScriptDTO>?> LoadingScriptContent(IEnumerable<ScriptDTO>? all);

    /// <summary>
    /// 绑定 JS
    /// </summary>
    /// <returns></returns>
    Task<bool> BuildScriptAsync(ScriptDTO model, FileInfo fileInfo, bool build = true);

    /// <summary>
    /// 添加 JS
    /// </summary>
    /// <param name="path"></param>
    /// <param name="oldInfo"></param>
    /// <param name="build"></param>
    /// <param name="order"></param>
    /// <param name="deleteFile"></param>
    /// <param name="pid"></param>
    /// <param name="ignoreCache"></param>
    /// <returns></returns>
    Task<IApiResponse<ScriptDTO?>> AddScriptAsync(string path, ScriptDTO? oldInfo = null, bool build = true, int? order = null, bool deleteFile = false, Guid? pid = null, bool ignoreCache = false);

    /// <summary>
    /// 添加 JS
    /// </summary>
    /// <param name="path"></param>
    /// <param name="info"></param>
    /// <param name="oldInfo"></param>
    /// <param name="build"></param>
    /// <param name="order"></param>
    /// <param name="deleteFile"></param>
    /// <param name="pid"></param>
    /// <param name="ignoreCache"></param>
    /// <returns></returns>
    Task<IApiResponse<ScriptDTO?>> AddScriptAsync(FileInfo path, ScriptDTO? info, ScriptDTO? oldInfo = null, bool build = true, int? order = null, bool deleteFile = false, Guid? pid = null, bool ignoreCache = false);

    /// <summary>
    /// 获取全部脚本
    /// </summary>
    /// <returns></returns>
    Task<IEnumerable<ScriptDTO>> GetAllScriptAsync();

    /// <summary>
    /// 删除脚本
    /// </summary>
    /// <param name="item"></param>
    /// <param name="removeByDataBase"></param>
    /// <returns></returns>
    Task<IApiResponse> DeleteScriptAsync(ScriptDTO item, bool removeByDataBase = true);

    /// <summary>
    /// 通过 Url 下载 JS
    /// </summary>
    /// <param name="url"></param>
    /// <returns></returns>
    Task<IApiResponse<string>> DownloadScriptAsync(string url);

    /// <summary>
    /// 保存脚本启用状态
    /// </summary>
    /// <param name="item"></param>
    /// <returns></returns>
    Task SaveEnableScript(ScriptDTO item);
}