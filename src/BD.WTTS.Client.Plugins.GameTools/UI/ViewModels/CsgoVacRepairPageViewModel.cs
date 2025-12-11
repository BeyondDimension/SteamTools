using static System.Net.Mime.MediaTypeNames;

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

    readonly ISteamService steamService = ISteamService.Instance;

    string BatPath = Path.Combine(Plugin.Instance.AppDataDirectory, "BAT", $"CSGOVAC_REPAIR{FileEx.BAT}");

    public async Task Repairs_Click()
    {
        try
        {
            Repairing = true;
            var installPath = $"\"{Path.GetDirectoryName(steamService.SteamProgramPath)}\\bin\"";
            await ExcuteBatCommand(installPath);
        }
        finally
        {
            Repairing = false;
        }
    }

    public async Task ExcuteBatCommand(string arg)
    {
        OutputString = string.Empty;
        if (!File.Exists(BatPath))
        {
            GenerateFixScript(BatPath);
        }

        using var p = new Process
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
                Arguments = arg,
                Verb = "runas",
            }
        };
        p.Start();
        _ = ConsumeReader(p.StandardOutput);
        _ = ConsumeReader(p.StandardError);
        await p.WaitForExitAsync();
    }

    async Task ConsumeReader(TextReader reader)
    {
        string? text;
        while ((text = await reader.ReadLineAsync()) != null)
        {
            OutHandle(text);
        }
    }

    void GenerateFixScript(string path)
    {
        var dirPath = Path.GetDirectoryName(path);
        if (!Directory.Exists(dirPath)) Directory.CreateDirectory(dirPath!);

        using var stream = new FileStream(path, FileMode.OpenOrCreate, FileAccess.Write, FileShare.ReadWrite | FileShare.Delete);
        stream.Write("""
                @echo off
                chcp 65001
                goto enableservice
                :steam
                echo Info - Checking if Steam is launched.
                tasklist | find /I "Steam.exe"
                if errorlevel 1 goto closedstatus
                if not errorlevel 1 goto killsteam
                
                :killsteam
                taskkill /F /IM Steam.exe
                goto steamrepair
                
                :closedstatus
                echo Info - Not Started
                goto steamrepair
                
                :enableservice
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
                
                :steamrepair
                echo Info - ※^>^>^> 执行修复启动器服务项
                cd /d %1
                steamservice /install
                ping -n 2 127.0.0.1>nul
                echo.
                steamservice /repair
                ping -n 2 127.0.0.1>nul
                echo Info - ※ 恢复DEP默认启动设置
                bcdedit /deletevalue nointegritychecks
                bcdedit /deletevalue loadoptions
                bcdedit /debug off
                bcdedit /deletevalue nx
                echo Info - ※^>^>^> 重启 Steam
                cd /d ..
                start /high steam
                ping -n 2 127.0.0.1>nul
                sc config "Steam Client Service" start= AUTO
                sc start "Steam Client Service"
                echo Info - ※ 执行完毕
                exit
                """u8);
        stream.Flush();
        stream.SetLength(stream.Position);
    }

    void OutHandle(string? msg)
    {
        if (!string.IsNullOrEmpty(msg))
        {
            OutputString += msg + Environment.NewLine;

            if (msg.Contains("Add firewall exception failed for steamservice.exe", StringComparison.OrdinalIgnoreCase))
            {
                OutputString += "info - ※ 修复 Steam Services 失败\n";
                OutputString += "info - ※ 请检查您的防火墙设置(关闭 \"不允许例外\" 选项)再次尝试\n";
            }
        }
    }

#endif
}
