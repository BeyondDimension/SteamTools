using System.Application.Services;

namespace System.Application
{
    /// <summary>
    /// 使用单元测试进行检查依赖注入关系是否配置正确
    /// </summary>
    static class DISafeGet
    {
        public static ISteamService GetLoginUsingSteamClientAuth()
        {
            Startup.Init(DILevel.HttpClientFactory | DILevel.Steam);
            return ISteamService.Instance;
        }
    }
}