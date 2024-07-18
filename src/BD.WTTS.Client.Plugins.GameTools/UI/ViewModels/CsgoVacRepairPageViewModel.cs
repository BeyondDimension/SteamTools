namespace BD.WTTS.UI.ViewModels;

public sealed partial class CsgoVacRepairPageViewModel : ViewModelBase
{
    string _OutputString = string.Empty;

    public string OutputString
    {
        get => _OutputString;
        set => this.RaiseAndSetIfChanged(ref _OutputString, value);
    }

    [Reactive]
    public bool Repairing { get; set; } = false;

#if WINDOWS

    string BatPath = Path.Combine(Plugin.Instance.AppDataDirectory, "BAT", $"CSGOVAC_REPAIR{FileEx.BAT}");

    public void Repairs_Click()
    {
        OutputString = string.Empty;
        Task2.InBackground(async () =>
        {
            await ExcuteBat();
        });
    }

    public async Task ExcuteBat()
    {
        try
        {
            if (!File.Exists(BatPath))
            {
                GenerateFixScript(BatPath);
            }
            Repairing = true;
            using var process = new Process
            {
                StartInfo =
                {
                    FileName = BatPath,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    StandardOutputEncoding = Encoding.UTF8,
                    StandardErrorEncoding = Encoding.UTF8,
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    Arguments = $"\"{Registry.GetValue(@"HKEY_CURRENT_USER\SOFTWARE\Valve\Steam", "SteamPath", null)}/bin\""
                }
            };

            process.OutputDataReceived += (_, e) =>
            {
                OutHandle(e.Data);
            };

            process.ErrorDataReceived += (_, e) =>
            {
                OutHandle(e.Data);
            };

            process.Start();
            process.BeginOutputReadLine();
            process.BeginErrorReadLine();
            await process.WaitForExitAsync();
        }
        finally
        {
            Repairing = false;
        }
    }

    static void GenerateFixScript(string path)
    {
        var dirPath = Path.GetDirectoryName(path);
        if (!Directory.Exists(dirPath)) Directory.CreateDirectory(dirPath!);

        using var stream = new FileStream(path, FileMode.OpenOrCreate, FileAccess.Write, FileShare.ReadWrite | FileShare.Delete);
        stream.Write("""
                @echo off
                chcp 65001
                :steam
                echo info - Checking if Steam is launched.
                tasklist | find /I "Steam.exe"
                if errorlevel 1 goto closedstatus
                if not errorlevel 1 goto killsteam
                :killsteam
                taskkill /F /IM Steam.exe
                goto steaminstall
                :closedstatus
                echo info - Not Started
                goto steaminstall
                sc config Netman start= AUTO
                sc start Netman
                sc config RasMan start= AUTO
                sc start RasMan
                sc config TapiSrv start= AUTO
                sc start TapiSrv
                sc config MpsSvc start= AUTO
                sc start MpsSvc
                netsh advfirewall set allprofiles state on
                goto steam
                :steaminstall
                echo info - ※^>^>^> 执行 Steam Services 修复中......
                cd /d %1
                steamservice /install
                ping -n 2 127.0.0.1>nul
                echo 
                steamservice /repair
                echo info - ※ 开启数据执行保护
                bcdedit /debug off
                bcdedit.exe/set {current} nx alwayson
                echo info - ※ 执行完毕
                sc config "Steam Client Service" start= AUTO
                sc start "Steam Client Service"
                """u8);
        stream.Flush();
        stream.SetLength(stream.Position);
    }

    void OutHandle(string? msg)
    {
        if (!string.IsNullOrEmpty(msg))
        {
            OutputString += $"{msg}\n";

            if (msg.Contains("Add firewall exception failed for steamservice.exe", StringComparison.OrdinalIgnoreCase))
            {
                OutputString += "info - ※ 修复 Steam Services 失败\n";
                OutputString += "info - ※ 请检查您的防火墙设置(关闭 \"不允许例外\" 选项)再次尝试\n";
            }
        }
    }

#endif
}
