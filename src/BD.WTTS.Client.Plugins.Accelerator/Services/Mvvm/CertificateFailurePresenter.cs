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
            var state = code == StartProxyResultCode.KeyIsolationServiceDisabled ? "已禁用" : "未运行";
            var answer = await MessageBox.ShowAsync(
                $"无法生成或加载加速证书：Windows 的 CNG Key Isolation 服务{state}。\n\n点击确定打开 Windows 服务设置，请将该服务设为手动并启动，然后返回重试。",
                "证书服务不可用", MessageBox.Button.OKCancel, MessageBox.Image.Warning);
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
            ? "加速证书未能安装或信任，请检查系统证书权限后重试。"
            : "无法生成或加载加速证书，请检查证书文件和系统加密服务。详细错误已记录在加速服务日志中。";
        await MessageBox.ShowAsync(message, "加速证书不可用", MessageBox.Button.OK, MessageBox.Image.Error);
    }
}
