using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Titanium.Web.Proxy.Network;

namespace Titanium.Web.Proxy.UnitTests
{
    [TestClass]
    public class CertificateManagerTests
    {
        private static readonly string[] hostNames
            = { "facebook.com", "youtube.com", "google.com", "bing.com", "yahoo.com" };


        [TestMethod]
        public async Task Simple_BC_Create_Certificate_Test()
        {
            var tasks = new List<Task>();

            var mgr = new CertificateManager(null, null, false, false, false, new Lazy<ExceptionHandler>(() => (e =>
            {
                Debug.WriteLine(e.ToString());
                Debug.WriteLine(e.InnerException?.ToString());
            })).Value)
            {
                CertificateEngine = CertificateEngine.BouncyCastle
            };
            mgr.ClearIdleCertificates();
            for (int i = 0; i < 5; i++)
            {
                tasks.AddRange(hostNames.Select(host => Task.Run(() =>
                {
                    // get the connection
                    var certificate = mgr.CreateCertificate(host, false);
                    Assert.IsNotNull(certificate);
                })));
            }

            await Task.WhenAll(tasks.ToArray());

            mgr.StopClearIdleCertificates();
        }

        // uncomment this to compare WinCert maker performance with BC (BC takes more time for same test above)
        //[TestMethod]
        public async Task Simple_Create_Win_Certificate_Test()
        {
            var tasks = new List<Task>();

            var mgr = new CertificateManager(null, null, false, false, false, new Lazy<ExceptionHandler>(() => (e =>
            {
                Debug.WriteLine(e.ToString());
                Debug.WriteLine(e.InnerException?.ToString());
            })).Value)
            { CertificateEngine = CertificateEngine.DefaultWindows };

            mgr.CreateRootCertificate();
            mgr.TrustRootCertificate(true);
            mgr.ClearIdleCertificates();

            for (int i = 0; i < 5; i++)
            {
                tasks.AddRange(hostNames.Select(host => Task.Run(() =>
                {
                    // get the connection
                    var certificate = mgr.CreateCertificate(host, false);
                    Assert.IsNotNull(certificate);
                })));
            }

            await Task.WhenAll(tasks.ToArray());
            mgr.RemoveTrustedRootCertificate(true);
            mgr.StopClearIdleCertificates();

        }
    }
}
