using System.Globalization;
using BD.WTTS.Client.Resources;
using BD.WTTS.Services;
using BD.WTTS.Services.Implementation;
using Microsoft.Extensions.Logging.Abstractions;

namespace BD.WTTS.UnitTest;

public sealed class ProxyLocalizationTest
{
    [Test]
    [NonParallelizable]
    public async Task DnsFailureMessageIncludesHostInNeutralAndSelectedCultures()
    {
        var originalCulture = Strings.Culture;
        try
        {
            var service = (IPCToastService)new IPCMainProcessServiceImpl(NullLoggerFactory.Instance);
            foreach (var culture in new[] { CultureInfo.InvariantCulture, CultureInfo.GetCultureInfo("en") })
            {
                Strings.Culture = culture;
                var template = await service.GetTextAsync(IPCToastService.ToastText.CommunityFix_DnsPollutionResponse);
                Assert.That(template, Does.Contain("{0}"));
                Assert.That(string.Format(template!, "example.test"), Does.Contain("example.test"));
            }
        }
        finally
        {
            Strings.Culture = originalCulture;
        }
    }
}
