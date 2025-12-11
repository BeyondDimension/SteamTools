using static BD.WTTS.Services.IDnsAnalysisService;

namespace BD.WTTS.UnitTest;

public sealed class DnsDohAnalysisTest
{
    //[Test]
    //public async Task DohAnalysisDomainIpTest()
    //{
    //    Ioc.ConfigureServices(static s =>
    //    {
    //        s.AddLogging(s => s.AddConsole());
    //        s.TryAddClientHttpPlatformHelperService();
    //        FusilladeHttpClientFactory2 factory = new();
    //        s.AddSingleton<IHttpClientFactory>(factory);
    //        s.AddSingleton<DnsDohAnalysisService>();
    //    });

    //    var dohAddress = new[]
    //    {
    //        Dnspod_DohAddres,
    //        Dnspod_DohAddres2,
    //        Dnspod_DohAddres3,
    //        DNS_Ali_DohAddres,
    //        DNS_Ali_DohAddres2,
    //        DNS_Ali_DohAddres3,
    //        //Google_DohAddres,
    //        //Cloudflare_DohAddres,
    //        DohAddres_360,
    //        TUNA_DohAddres,
    //    };

    //    var s = Ioc.Get<DnsDohAnalysisService>();
    //    ConcurrentBag<string> strings = new();
    //    async ValueTask Query(string dohAddres, CancellationToken cancellationToken = default)
    //    {
    //        var b = new StringBuilder();
    //        b.AppendLine(dohAddres);
    //        try
    //        {
    //            var result = await s.DohAnalysisDomainIpAsync(dohAddres, "github.rmbgame.net", true, cancellationToken: cancellationToken).ToArrayAsync(cancellationToken);
    //            b.AppendLine(
    //                string.Join(Environment.NewLine, result.Select(x => x.ToString())));
    //        }
    //        catch (Exception ex)
    //        {
    //            b.AppendLine(ex.ToString());
    //        }
    //        finally
    //        {
    //            b.AppendLine();
    //        }
    //        var str = b.ToString();
    //        strings.Add(str);
    //    }

    //    await Parallel.ForEachAsync(dohAddress, Query);

    //    foreach (var str in strings)
    //    {
    //        Console.WriteLine("--------------------");
    //        Console.WriteLine(str);
    //        Console.WriteLine("--------------------");
    //    }
    //}
}

sealed class FusilladeHttpClientFactory2 : FusilladeHttpClientFactory
{
    protected override HttpClient CreateClient(HttpMessageHandler handler)
    {
        var client = new HttpClient(handler);

        try
        {
            client.Timeout = TimeSpan.FromSeconds(14.5D);
        }
        catch
        {

        }

        return client;
    }
}