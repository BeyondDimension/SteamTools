using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace SteamTool.Proxy.Test
{
    [TestClass]
    public class TestDnsResolution
    {
        [TestMethod]
        public void TestResolutionDomainIp()
        {
            var result = DnsResolution.ResolutionDomainIp("steamcommunity-a.akamaihd.net");
            Console.WriteLine(result.ToString());
        }

        [TestMethod]
        public void TestResolutionDomainIpByGoogleDns()
        {
            var result = DnsResolution.ResolutionDomainIpByGoogleDns("steamcommunity-a.akamaihd.net");
            Console.WriteLine(result.ToString());
        }

    }
}
