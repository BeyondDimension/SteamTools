// ReSharper disable once CheckNamespace
namespace BD.WTTS;

/// <summary>
/// NixOS 证书链路的一次性诊断输出。
/// 应用的日志目录依赖 XDG_CACHE_HOME，证书失败时往往正好没有日志可看，
/// 因此这里直接往 /tmp 追加一份最小事实（进程、AppData、各证书路径状态、异常）。
/// 仅在 Linux 证书相关失败路径调用，正常流程不产生任何 IO。
/// </summary>
public static class CertDiagnostics
{
    public const string FilePath = "/tmp/watt-toolkit-cert-diag.log";

    public static void Write(string stage, Exception? ex = null)
    {
        try
        {
            var sb = new System.Text.StringBuilder();
            sb.AppendLine($"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] {stage}");
            sb.AppendLine($"  pid={Environment.ProcessId} exe={Environment.ProcessPath}");

            static string Env(string name)
                => Environment.GetEnvironmentVariable(name) is { Length: > 0 } v ? v : "<null>";

            sb.AppendLine($"  XDG_DATA_HOME={Env("XDG_DATA_HOME")}");
            sb.AppendLine($"  XDG_CACHE_HOME={Env("XDG_CACHE_HOME")}");
            sb.AppendLine($"  HOME={Env("HOME")}");
            sb.AppendLine($"  STEAMTOOLS_BUNDLED_CER={Env("STEAMTOOLS_BUNDLED_CER")}");
            sb.AppendLine($"  AppData(CertificateConstants)={CertificateConstants.AppDataDirectory}");
            sb.AppendLine($"  IOPath.AppDataDirectory={IOPath.AppDataDirectory}");

            AppendFile(sb, "pfx(默认路径)", CertificateConstants.DefaultPfxFilePath);
            AppendFile(sb, "cer(默认路径)", CertificateConstants.DefaultCerFilePath);
            AppendFile(sb, "cer(导出目标)", Environment.GetEnvironmentVariable("STEAMTOOLS_BUNDLED_CER"));

            // 另一套可能的 AppData（wrapper 设置 XDG_DATA_HOME 后的目录），用于核对两个进程是否一致
            var home = Environment.GetEnvironmentVariable("HOME");
            if (!string.IsNullOrWhiteSpace(home))
            {
                var wrapperAppData = Path.Combine(home, ".local", "share", "WattToolkit", "Steam++");
                AppendFile(sb, "pfx(wrapper AppData)", Path.Combine(wrapperAppData, CertificateConstants.PfxFileName));
                AppendFile(sb, "cer(wrapper AppData)", Path.Combine(wrapperAppData, CertificateConstants.CerFileName));
            }

            if (ex != null)
            {
                sb.AppendLine("  exception:");
                foreach (var line in ex.ToString().Split('\n'))
                    sb.AppendLine("    " + line.TrimEnd('\r'));
            }

            File.AppendAllText(FilePath, sb.AppendLine().ToString());
        }
        catch
        {
            // 诊断本身绝不能影响主流程
        }
    }

    static void AppendFile(System.Text.StringBuilder sb, string name, string? path)
    {
        if (string.IsNullOrWhiteSpace(path))
        {
            sb.AppendLine($"  {name}: <未设置>");
            return;
        }

        try
        {
            var fi = new FileInfo(path);
            if (!fi.Exists)
            {
                sb.AppendLine($"  {name}: 不存在 {path}");
                return;
            }

            string mode;
            try
            {
                mode = ((int)File.GetUnixFileMode(path)).ToString();
            }
            catch
            {
                mode = "?";
            }

            sb.AppendLine($"  {name}: {path} size={fi.Length} mtime={fi.LastWriteTimeUtc:O} mode={mode}");
        }
        catch (Exception e)
        {
            sb.AppendLine($"  {name}: 读取失败 {path} ({e.GetType().Name}: {e.Message})");
        }
    }
}
