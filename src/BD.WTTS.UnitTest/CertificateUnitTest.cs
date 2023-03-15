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
    public void TestGenerateByCa()
    {
        using var certificate2 = CertGenerator.GenerateByCaPfx(new[] { "steampp.net", "www.steampp.net", }, default, DateTimeOffset.UtcNow.AddYears(1), "ca.pfx");
        byte[] exported = certificate2.Export(X509ContentType.Cert);
        File.WriteAllBytes("steampp.net.cer", exported);
    }
}
