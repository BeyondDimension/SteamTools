using Avalonia.Threading;
using ReactiveUI.Reactive;
using System.Reactive.Linq;
using System.Windows.Input;

namespace BD.WTTS.UI.ViewModels;

public sealed partial class CsgoVacRepairPageViewModel : ViewModelBase
{
    static int repairInProgress;
    string _OutputString = string.Empty;
    bool _Repairing;

    public CsgoVacRepairPageViewModel()
    {
        var repair = ReactiveCommand.CreateFromTask(Repairs_Click, Observable.Return(IsRepairSupported));
        repair.AddTo(this);
        RepairCommand = repair;
    }

    public ICommand RepairCommand { get; }

    public bool IsRepairSupported
    {
        get
        {
#if WINDOWS
            return OperatingSystem.IsWindows();
#else
            return false;
#endif
        }
    }

    public string OutputString
    {
        get => _OutputString;
        set
        {
            if (_OutputString == value) return;
            this.RaisePropertyChanging(nameof(OutputString));
            _OutputString = value;
            this.RaisePropertyChanged(nameof(OutputString));
        }
    }

    public bool Repairing
    {
        get => _Repairing;
        set
        {
            if (_Repairing == value) return;
            this.RaisePropertyChanging(nameof(Repairing));
            _Repairing = value;
            this.RaisePropertyChanged(nameof(Repairing));
        }
    }

    public Task Repairs_Click() => RunRepairAsync(null);

    async Task RunRepairAsync(string? arg)
    {
        // The script changes shared system state, so also guard calls from another page instance.
        if (Interlocked.CompareExchange(ref repairInProgress, 1, 0) != 0) return;

        try
        {
            await Dispatcher.UIThread.InvokeAsync(() =>
            {
                Repairing = true;
                OutputString = string.Empty;
            });
#if WINDOWS
            arg ??= $"\"{Path.GetDirectoryName(ISteamService.Instance.SteamProgramPath)}\\bin\"";
            await ExecuteScriptAsync(arg).ConfigureAwait(false);
#else
            throw new PlatformNotSupportedException();
#endif
        }
        catch (Exception ex)
        {
            Log.Error(nameof(CsgoVacRepairPageViewModel), ex, "VAC repair failed.");
            await Dispatcher.UIThread.InvokeAsync(() =>
                OutHandle(string.Format(CultureInfo.CurrentCulture, Strings.Error_Exception_, ex.Message)));
        }
        finally
        {
            try
            {
                await Dispatcher.UIThread.InvokeAsync(() => Repairing = false);
            }
            finally
            {
                Interlocked.Exchange(ref repairInProgress, 0);
            }
        }
    }

#if WINDOWS

    readonly string BatPath = Path.Combine(Plugin.Instance.AppDataDirectory, "BAT", $"CSGOVAC_REPAIR{FileEx.BAT}");

    public Task ExcuteBatCommand(string arg) => RunRepairAsync(arg);

    async Task ExecuteScriptAsync(string arg)
    {
        if (!File.Exists(BatPath))
        {
            GenerateFixScript(BatPath);
        }

        using var p = new Process
        {
            StartInfo =
            {
                FileName = Path.Combine(Environment.SystemDirectory, "cmd.exe"),
                // /s strips only the outer quotes, preserving quoted batch and Steam paths.
                Arguments = $"/d /s /v:off /c \"\"{BatPath}\" {arg}\"",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                StandardOutputEncoding = Encoding.UTF8,
                StandardErrorEncoding = Encoding.UTF8,
                UseShellExecute = false,
                CreateNoWindow = true,
            }
        };
        p.Start();
        var output = ConsumeReader(p.StandardOutput);
        var error = ConsumeReader(p.StandardError);
        await Task.WhenAll(output, error, p.WaitForExitAsync()).ConfigureAwait(false);
        if (p.ExitCode != 0)
        {
            throw new InvalidOperationException($"VAC repair script exited with code {p.ExitCode}.");
        }
    }

    async Task ConsumeReader(TextReader reader)
    {
        string? text;
        while ((text = await reader.ReadLineAsync().ConfigureAwait(false)) != null)
        {
            var line = text;
            await Dispatcher.UIThread.InvokeAsync(() => OutHandle(line));
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

#endif

    void OutHandle(string? msg)
    {
        if (!string.IsNullOrEmpty(msg))
        {
            OutputString += (msg switch
            {
                "Info - ※>>> 执行修复启动器服务项" => Strings.L10nGames_RepairLauncherService,
                "Info - ※ 恢复DEP默认启动设置" => Strings.L10nGames_RestoreDep,
                "Info - ※>>> 重启 Steam" => Strings.L10nGames_RestartSteam,
                "Info - ※ 执行完毕" => Strings.L10nGames_RepairComplete,
                _ => msg,
            }) + Environment.NewLine;

            if (msg.Contains("Add firewall exception failed for steamservice.exe", StringComparison.OrdinalIgnoreCase))
            {
                OutputString += Strings.L10nGames_RepairServicesFailed;
                OutputString += Strings.L10nGames_RepairFirewall;
            }
        }
    }

}
