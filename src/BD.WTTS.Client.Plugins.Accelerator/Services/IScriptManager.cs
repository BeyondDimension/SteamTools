// ReSharper disable once CheckNamespace
namespace BD.WTTS.Services;

public interface IScriptManager
{
    const string DirName = Migrations.DirName_Scripts;
    const string DirName_Build = Migrations.DirName_BuildScripts;

    static IScriptManager Instance => Ioc.Get<IScriptManager>();

    /// <summary>
    /// 加载列表 JS 内容
    /// </summary>
    /// <param name="all">加载的列表</param>
    /// <returns></returns>
    Task<IEnumerable<ScriptDTO>?> LoadingScriptContentAsync(IEnumerable<ScriptDTO>? all);

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
    /// <param name="isCompile"></param>
    /// <param name="order"></param>
    /// <param name="deleteFile"></param>
    /// <param name="pid"></param>
    /// <param name="ignoreCache"></param>
    /// <returns></returns>
    Task<IApiRsp<ScriptDTO?>> AddScriptAsync(string path, ScriptDTO? oldInfo = null, bool isCompile = true, long? order = null, bool deleteFile = false, Guid? pid = null, bool ignoreCache = false);

    /// <summary>
    /// 读取文件转 DTO
    /// </summary>
    /// <param name="path"></param>
    /// <returns></returns>
    Task<ScriptDTO?> ReadScriptAsync(string path);

    /// <summary>
    /// 保存 JS
    /// </summary>
    /// <param name="path"></param>
    /// <param name="info"></param>
    /// <param name="oldInfo"></param>
    /// <param name="isCompile"></param>
    /// <param name="order"></param>
    /// <param name="deleteFile"></param>
    /// <param name="pid"></param>
    /// <param name="ignoreCache"></param>
    /// <returns></returns>
    Task<IApiRsp<ScriptDTO?>> SaveScriptAsync(FileInfo path, ScriptDTO? info, ScriptDTO? oldInfo = null, bool isCompile = true, long? order = null, bool deleteFile = false, Guid? pid = null, bool ignoreCache = false);

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
    Task<IApiRsp> DeleteScriptAsync(ScriptDTO item, bool removeByDataBase = true);

    /// <summary>
    /// 通过 Url 下载 JS
    /// </summary>
    /// <param name="url"></param>
    /// <returns></returns>
    Task<IApiRsp<string?>> DownloadScriptAsync(string url);

    /// <summary>
    /// 保存脚本启用状态
    /// </summary>
    /// <param name="item"></param>
    /// <returns></returns>
    Task SaveEnableScriptAsync(ScriptDTO item);

    /// <summary>
    /// 检查脚本文件是否存在
    /// </summary>
    /// <param name="list"></param>
    /// <returns></returns>
    Task<IEnumerable<ScriptDTO>> CheckFiles(IEnumerable<ScriptDTO> list);
}