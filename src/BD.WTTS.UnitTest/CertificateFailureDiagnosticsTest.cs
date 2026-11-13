using System.Reflection;
using System.Security.Cryptography;
using BD.WTTS.Services.Implementation;
using BD.WTTS.Services;

namespace BD.WTTS.UnitTest;

public sealed class CertificateFailureDiagnosticsTest
{
    [Test]
    public void IpcResultCodesAreUniqueAndPreserveIcuCode()
    {
        var values = Enum.GetValues<StartProxyResultCode>();
        Assert.That(values.Distinct().Count(), Is.EqualTo(values.Length));
        Assert.That((byte)StartProxyResultCode.IcuTestFail, Is.EqualTo(127));
    }

    [Test]
    public void FrontendRecognizesCertificateErrorsWithoutMistakingIcuFailureForOne()
    {
        var type = typeof(StartProxyResultCode).Assembly.GetType("BD.WTTS.Services.CertificateFailurePresenter");
        var method = type!.GetMethod("IsCertificateFailure", BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
        Assert.That(method, Is.Not.Null);
        bool Recognizes(StartProxyResultCode code) => (bool)method!.Invoke(null, new object[] { code })!;

        Assert.Multiple(() =>
        {
            Assert.That(Recognizes(StartProxyResultCode.KeyIsolationServiceDisabled), Is.True);
            Assert.That(Recognizes(StartProxyResultCode.KeyIsolationServiceStopped), Is.True);
            Assert.That(Recognizes(StartProxyResultCode.CertificateOperationFailed), Is.True);
            Assert.That(Recognizes(StartProxyResultCode.IcuTestFail), Is.False);
        });
    }

    [Test]
    public void ReportsStoppedServiceWhenItStartsDuringTheFailingCertificateOperation()
    {
        var type = typeof(CertGenerator).Assembly.GetType("BD.WTTS.Services.CertificateFailureDiagnostics");
        var method = type!.GetMethod("ClassifyWithPreviousState", BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
        Assert.That(method, Is.Not.Null, "The service state before the operation must be considered.");
        var result = method!.Invoke(null, new object?[] { new CryptographicException("key"), true, 3, 1, 3, 4 });
        Assert.That(result!.ToString(), Is.EqualTo("KeyIsolationStopped"));
    }
    [Test]
    public void OnlyIdentifiesKeyIsolationWhenCryptographyFailsAndServiceStateSupportsIt()
    {
        var type = typeof(CertGenerator).Assembly.GetType("BD.WTTS.Services.CertificateFailureDiagnostics");
        Assert.That(type, Is.Not.Null, "Certificate failure classification must be available.");
        var method = type!.GetMethod("Classify", BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
        Assert.That(method, Is.Not.Null);

        string Classify(Exception exception, bool windows, int? startupType, int? state) =>
            method!.Invoke(null, new object?[] { exception, windows, startupType, state })!.ToString()!;

        Assert.Multiple(() =>
        {
            Assert.That(Classify(new CryptographicException("key"), true, 4, 1), Is.EqualTo("KeyIsolationDisabled"));
            Assert.That(Classify(new CryptographicException("key"), true, 3, 1), Is.EqualTo("KeyIsolationStopped"));
            Assert.That(Classify(new CryptographicException("key"), true, 3, 4), Is.EqualTo("CertificateFailure"));
            Assert.That(Classify(new CryptographicException("key"), true, null, null), Is.EqualTo("CertificateFailure"));
            Assert.That(Classify(new IOException("disk"), true, 4, 1), Is.EqualTo("CertificateFailure"));
            Assert.That(Classify(new CryptographicException("key"), false, 4, 1), Is.EqualTo("CertificateFailure"));
            Assert.That(Classify(new Exception("wrapper", new CryptographicException("key")), true, 4, 1), Is.EqualTo("KeyIsolationDisabled"));
        });
    }
}
