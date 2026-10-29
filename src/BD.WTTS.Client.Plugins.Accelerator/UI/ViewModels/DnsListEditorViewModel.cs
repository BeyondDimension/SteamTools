using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Net;
using System.Reactive;
using BD.WTTS.Services;
using BD.WTTS.UI.Views.Controls;

namespace BD.WTTS.UI.ViewModels;

/// <summary>Edits the persisted, ordered server list shared by the settings and diagnostics views.</summary>
public sealed partial class DnsListEditorViewModel : ViewModelBase
{
    public static DnsListEditorViewModel ServerList { get; } = new();

    public ObservableCollection<string> Servers { get; } = new();

    [Reactive]
    public string Input { get; set; } = string.Empty;

    [Reactive]
    public string Error { get; set; } = string.Empty;

    [Reactive]
    public int SelectedIndex { get; set; } = -1;

    public ReactiveCommand<Unit, Unit> OpenManagerCommand { get; }

    public ReactiveCommand<Unit, Unit> AddCommand { get; }
    public ReactiveCommand<Unit, Unit> AddSystemDefaultCommand { get; }
    public ReactiveCommand<Unit, Unit> RemoveCommand { get; }
    public ReactiveCommand<Unit, Unit> MoveUpCommand { get; }
    public ReactiveCommand<Unit, Unit> MoveDownCommand { get; }

    DnsListEditorViewModel()
    {
        ProxySettings.MigrateDnsServerList();
        OpenManagerCommand = ReactiveCommand.Create(OpenManager);
        AddCommand = ReactiveCommand.Create(Add);
        AddSystemDefaultCommand = ReactiveCommand.Create(() => AddValue("System Default"));

        var count = Observable.FromEventPattern<NotifyCollectionChangedEventHandler, NotifyCollectionChangedEventArgs>(
                handler => Servers.CollectionChanged += handler,
                handler => Servers.CollectionChanged -= handler)
            .Select(_ => Servers.Count)
            .StartWith(Servers.Count);
        var selected = this.WhenAnyValue(x => x.SelectedIndex);
        RemoveCommand = ReactiveCommand.Create(Remove, selected.CombineLatest(count, (index, total) => index >= 0 && index < total));
        MoveUpCommand = ReactiveCommand.Create(() => Move(-1), selected.CombineLatest(count, (index, total) => index > 0 && index < total));
        MoveDownCommand = ReactiveCommand.Create(() => Move(1), selected.CombineLatest(count, (index, total) => index >= 0 && index < total - 1));
        ProxySettings.ProxyMasterDns.Subscribe(_ => Reload(), false);
        Reload();
    }

    void OpenManager()
    {
        Input = string.Empty;
        Error = string.Empty;
        SelectedIndex = Servers.Count > 0 ? 0 : -1;
        _ = IWindowManager.Instance.ShowTaskDialogAsync(this, "管理 DNS / DoH 服务器",
            pageContent: new DnsListManager { DataContext = this }, okButtonText: "完成");
    }

    void Reload()
    {
        var value = ProxySettings.ProxyMasterDns.Value;
        if (string.IsNullOrWhiteSpace(value))
            value = IDnsAnalysisService.DefaultResolverServerList;
        var oldIndex = SelectedIndex;
        Servers.Clear();
        foreach (var server in value.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries))
        {
            var item = server.Trim();
            if (item.Length != 0)
                Servers.Add(item);
        }
        SelectedIndex = oldIndex < Servers.Count ? oldIndex : Servers.Count - 1;
    }

    void Add()
    {
        var value = Input?.Trim() ?? string.Empty;
        if (value.Contains('\r') || value.Contains('\n'))
        {
            Error = "一次只能添加一个服务器地址。";
            return;
        }
        if (!IPAddress.TryParse(value, out _) &&
            !string.Equals(value, "System Default", StringComparison.OrdinalIgnoreCase) &&
            (!Uri.TryCreate(value, UriKind.Absolute, out var uri) || uri.Scheme != Uri.UriSchemeHttps || string.IsNullOrEmpty(uri.Host)))
        {
            Error = "请输入有效的 IPv4、IPv6 地址、HTTPS DoH 地址或选择 System Default。";
            return;
        }
        if (string.Equals(value, "System Default", StringComparison.OrdinalIgnoreCase))
            value = "System Default";
        AddValue(value);
        Input = string.Empty;
    }

    void AddValue(string value)
    {
        Error = string.Empty;
        Servers.Add(value);
        SelectedIndex = Servers.Count - 1;
        Save();
    }

    void Remove()
    {
        if (SelectedIndex < 0 || SelectedIndex >= Servers.Count)
        {
            Error = "请先选择一个服务器。";
            return;
        }
        var index = SelectedIndex;
        Servers.RemoveAt(index);
        SelectedIndex = Math.Min(index, Servers.Count - 1);
        Error = string.Empty;
        Save();
    }

    void Move(int direction)
    {
        var index = SelectedIndex;
        var target = index + direction;
        if (index < 0 || index >= Servers.Count || target < 0 || target >= Servers.Count)
            return;
        Servers.Move(index, target);
        SelectedIndex = target;
        Error = string.Empty;
        Save();
    }

    void Save()
    {
        ProxySettings.ProxyMasterDns.Value = Servers.Count == 0
            ? IDnsAnalysisService.DefaultResolverServerList
            : string.Join("\n", Servers);
        if (Servers.Count == 0)
            Reload();
    }
}
