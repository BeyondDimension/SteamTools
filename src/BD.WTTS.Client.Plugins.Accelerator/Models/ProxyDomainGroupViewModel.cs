using FluentAvalonia.Core;
using System.Reactive;

namespace BD.WTTS.Models;

public class ProxyDomainGroupViewModel : ReactiveObject
{
    private readonly INetworkTestService networkTestService = INetworkTestService.Instance;

    public string Name { get; set; } = string.Empty;

    public string IconUrl { get; set; } = string.Empty;

    public ReadOnlyCollection<ProxyDomainViewModel>? EnableProxyDomainVMs { get; set; }

    public ReactiveCommand<Unit, Unit> ConnectTestCommand { get; set; }

    public ProxyDomainGroupViewModel()
    {
        ConnectTestCommand = ReactiveCommand.CreateFromTask(async () =>
        {
            if (EnableProxyDomainVMs == null)
            {
                Toast.Show(ToastIcon.Warning, Strings.L10nAccel_074);
                return;
            }

            var tasks = EnableProxyDomainVMs.Select(async enableDomain =>
            {
                enableDomain.DelayMillseconds = "- ms";
                var (success, delayMs) = await networkTestService.TestOpenUrlAsync(enableDomain.Url);
                enableDomain.DelayMillseconds = success switch
                {
                    true when delayMs > 20000 => "Timeout",
                    true => delayMs.ToString() + " ms",
                    false => "error",
                };
            });

            await Task.WhenAll(tasks);

            if (EnableProxyDomainVMs.Count(x => x.DelayMillseconds == "Timeout") == EnableProxyDomainVMs.Count)
            {
                Toast.Show(ToastIcon.Error, Strings.L10nAccel_075);
            }
        });
    }
}
