using Avalonia.Media;

namespace BD.WTTS.Models;

public class ProxyDomainViewModel : ReactiveObject
{
    private readonly INetworkTestService networkTestService = INetworkTestService.Instance;

    public string Name { get; }

    public ProxyType ProxyType { get; }

    public string Url { get; }

    [Reactive]
    public string DelayMillseconds { get; set; } = string.Empty;

    [ObservableAsProperty]
    public bool RetestButtonVisible { get; }

    [ObservableAsProperty]
    public IBrush DelayColor { get; } = null!;

    public ReadOnlyCollection<ProxyDomainViewModel>? Children { get; }

    public ProxyDomainViewModel(string name, ProxyType proxyType, string url, List<ProxyDomainViewModel>? children = null)
    {
        Name = name;
        ProxyType = proxyType;
        Url = url;
        Children = children?.AsReadOnly();

        this.WhenAnyValue(x => x.DelayMillseconds)
            .Subscribe(delay => Children?.ForEach(child => child.DelayMillseconds = delay));

        this.WhenAnyValue(x => x.DelayMillseconds)
            .Select(d => d != string.Empty && d != "-")
            .ToPropertyEx(this, x => x.RetestButtonVisible);

        const int DelayMiddle = 1000;
        this.WhenAnyValue(x => x.DelayMillseconds)
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
            .ToPropertyEx(this, x => x.DelayColor);
    }
}
