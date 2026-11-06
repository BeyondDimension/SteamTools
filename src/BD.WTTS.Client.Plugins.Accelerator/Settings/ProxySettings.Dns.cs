namespace BD.WTTS.Settings;

public static partial class ProxySettings
{
    internal static void MigrateDnsServerList()
    {
        lock (Abstractions.SettingsPropertyBase.GetSynchronizationRoot<ProxySettings_>())
            MigrateDnsServerListCore();
    }

    static void MigrateDnsServerListCore()
    {
        if (ResolverServersMigrated.Value)
            return;

        var dns = ProxyMasterDns.Value;
        if (string.IsNullOrWhiteSpace(dns) || dns == Services.IDnsAnalysisService.DefaultResolverServerList)
            dns = Services.IDnsAnalysisService.DefaultDnsServerList;
        var doh = CustomDohAddres2.Value;
        if (string.IsNullOrWhiteSpace(doh))
            doh = Services.IDnsAnalysisService.DefaultDohServerList;

        var servers = new List<string>();
        var seen = new HashSet<string>(StringComparer.Ordinal);
        void Append(string value)
        {
            foreach (var item in value.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries))
            {
                var address = Services.IDnsAnalysisService.NormalizeLegacyDohEndpoint(item.Trim());
                if (address.Length != 0 && seen.Add(address))
                    servers.Add(address);
            }
        }

        if (UseDoh.Value)
        {
            Append(doh);
            Append(dns);
        }
        else
        {
            Append(dns);
            Append(doh);
        }

        ProxyMasterDns.Value = string.Join("\n", servers);
        ResolverServersMigrated.Value = true;
    }
}
