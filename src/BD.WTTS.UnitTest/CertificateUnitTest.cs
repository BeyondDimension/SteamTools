using System.DirectoryServices.ActiveDirectory;

namespace BD.WTTS.UnitTest;

public sealed class CertificateUnitTest
{
    [SetUp]
    public void Setup()
    {
        TestContext.WriteLine(AppContext.BaseDirectory);
        using var certificate2 = CertGenerator.GenerateBySelfPfx(null, default, DateTimeOffset.UtcNow.AddYears(1), "ca.pfx");
    }

    [Test]
    [Ignore("https://github.com/dotnet/runtime/issues/82693")]
    public void TestGenerateByCa()
    {
        var domains = new[] { "steampp.net", "www.steampp.net", };
        var subjectName = new X500DistinguishedName($"CN={domains.First()}");
        var x509Certificate2 = new X509Certificate2("ca.pfx");
        using var certificate2 = CertGenerator.CreateEndCertificate(x509Certificate2, subjectName, domains, default, DateTimeOffset.UtcNow.AddYears(1));
        byte[] exported = certificate2.Export(X509ContentType.Cert);
        File.WriteAllBytes("steampp.net.cer", exported);
    }
}
