using BD.WTTS.Client.Resources;

namespace BD.WTTS.UI.ViewModels;

public partial class ScriptPageViewModel : TabItemViewModel
{
    public override string Name => Strings.Welcome;

    protected readonly ReadOnlyObservableCollection<ScriptDTO>? _ProxyScripts;

    public ReadOnlyObservableCollection<ScriptDTO>? ProxyScripts => _ProxyScripts;

    public bool IsProxyScriptsEmpty => !ProxyScripts.Any_Nullable();
}
