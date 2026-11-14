namespace BD.WTTS.Services;

internal static class CertificateFailurePresenter
{
    internal static bool IsCertificateFailure(StartProxyResultCode code) => code is
        StartProxyResultCode.KeyIsolationServiceDisabled or
        StartProxyResultCode.KeyIsolationServiceStopped or
        StartProxyResultCode.CertificateOperationFailed or
        StartProxyResultCode.GenerateCertificateFail or
        StartProxyResultCode.GenerateCerFilePathFail or
        StartProxyResultCode.GetCertificatePackableFail or
        StartProxyResultCode.GetX509Certificate2Fail or
        StartProxyResultCode.TrustRootCertificateFail;

    internal static async Task ShowAsync(StartProxyResultCode code)
    {
        if (OperatingSystem.IsWindows() &&
            code is (StartProxyResultCode.KeyIsolationServiceDisabled or StartProxyResultCode.KeyIsolationServiceStopped))
        {
            var state = code == StartProxyResultCode.KeyIsolationServiceDisabled ? Strings.L10nAccel_086 : Strings.L10nAccel_087;
            var answer = await MessageBox.ShowAsync(
                string.Format(Strings.L10nAccel_088, state),
                Strings.L10nAccel_089, MessageBox.Button.OKCancel, MessageBox.Image.Warning);
            if (answer.IsOK())
            {
                try
                {
                    Process.Start(new ProcessStartInfo("services.msc") { UseShellExecute = true });
                }
                catch (Exception ex)
                {
                    ex.LogAndShowT();
                }
            }
            return;
        }

        var message = code == StartProxyResultCode.TrustRootCertificateFail
            ? Strings.L10nAccel_090
            : Strings.L10nAccel_091;
        await MessageBox.ShowAsync(message, Strings.L10nAccel_092, MessageBox.Button.OK, MessageBox.Image.Error);
    }
}
