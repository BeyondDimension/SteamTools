using Avalonia.Media;

namespace BD.WTTS.Models;

public partial class ProxyDomainViewModel : ReactiveObject
{
    private readonly INetworkTestService networkTestService = INetworkTestService.Instance;

    public string Name { get; }

    public ProxyType ProxyType { get; }

    public string Url { get; }

    [Reactive]
    public partial string DelayMillseconds { get; set; } = string.Empty;

    readonly ObservableAsPropertyHelper<bool> retestButtonVisible;
    readonly ObservableAsPropertyHelper<IBrush> delayColor;

    public bool RetestButtonVisible => retestButtonVisible?.Value ?? false;

    public IBrush DelayColor => delayColor?.Value!;

    public ReadOnlyCollection<ProxyDomainViewModel>? Children { get; }

    public ProxyDomainViewModel(string name, ProxyType proxyType, string url, List<ProxyDomainViewModel>? children = null)
    {
        Name = name;
        ProxyType = proxyType;
        Url = url;
        Children = children?.AsReadOnly();

        this.ObserveValue(nameof(DelayMillseconds), static x => x.DelayMillseconds)
            .Subscribe(delay => Children?.ForEach(child => child.DelayMillseconds = delay));

        retestButtonVisible = this.ObserveValue(nameof(DelayMillseconds), static x => x.DelayMillseconds)
            .Select(d => d != string.Empty && d != "-")
            .ToProperty(this, nameof(RetestButtonVisible));

        const int DelayMiddle = 1000;
        delayColor = this.ObserveValue(nameof(DelayMillseconds), static x => x.DelayMillseconds)
            .Select(d => d switch
            {
                "Timeout" or "error" => Brushes.Red,
                var s when s.Split(' ') is [var num, "ms"] && int.TryParse(num, out int ms)
                   => ms switch
                   {
                       <= DelayMiddle => Brushes.Green,
                       > DelayMiddle => Brushes.Orange,
                   },
                _ => Brushes.Gray,
            })
            .ToProperty<ProxyDomainViewModel, IBrush>(this, nameof(DelayColor));
    }
}
