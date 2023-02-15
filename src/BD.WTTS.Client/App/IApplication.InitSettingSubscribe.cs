// ReSharper disable once CheckNamespace
namespace BD.WTTS;

partial interface IApplication
{
    /// <summary>
    /// 初始化设置项变更时监听
    /// </summary>
    void InitSettingSubscribe()
    {
        UISettings.Theme.Subscribe(x => Theme = (AppTheme)x);
        UISettings.Language.Subscribe(ResourceService.ChangeLanguage);
    }

    /// <inheritdoc cref="IApplication.InitSettingSubscribe"/>
    void PlatformInitSettingSubscribe() => InitSettingSubscribe();
}