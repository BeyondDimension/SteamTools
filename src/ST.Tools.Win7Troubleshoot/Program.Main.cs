using System;
using System.Application;
using System.Properties;
using System.Threading;
using System.Windows.Forms;
using static System.Program;

var t = new Thread(() =>
{
    mutex = new Mutex(true, currentProcess.ProcessName, out var isNotRunning);
    if (isNotRunning)
    {
        try
        {
            if (Environment.OSVersion.Platform != PlatformID.Win32NT)
                throw new PlatformNotSupportedException();

            FileSystemDesktop.InitFileSystem();

            thisFilePath = currentProcess.MainModule.FileName;
            //if (args.Length == 2 && string.Equals(args[0], "-start", StringComparison.OrdinalIgnoreCase) && string.Equals(args[1], "setup", StringComparison.OrdinalIgnoreCase)) // -start setup
            //{
            //    var fileName = $"setup.{(EnvironmentEx.Is64BitOperatingSystem ? "x64" : "x86")}.exe";
            //    var baseDir = Path.GetDirectoryName(thisFilePath);
            //    appFilePath = Path.Combine(baseDir, fileName);
            //}
            //else
            //{
            appFilePath = thisFilePath.Replace(".win7", string.Empty, StringComparison.OrdinalIgnoreCase);
            //}

            var isQuickStart = QuickStart(out var writeQuickStart);

            if (!isQuickStart)
            {
                var r = IsWin7SP1OrNotSupportedPlatform(out var error);
                if (r.HasValue)
                {
                    if (r.Value) // Win7SP1
                    {
                        if (!CheckInstalled_KB3063858())
                        {
                            if (MessageBox.Show(
                                SR.Not_Installed_KB3063858,
                                SR.Error,
                                MessageBoxButtons.YesNo,
                                MessageBoxIcon.Error) == DialogResult.Yes)
                            {
                                Open_KB3063858_DownloadLink();
                            }
                            return;
                        }
                    }
                    else // NotSupportedPlatform - For example, WinXP/Win2000
                    {
                        MessageBox.Show(
                            error,
                            SR.Error,
                            MessageBoxButtons.OK,
                            MessageBoxIcon.Error);
                        return;
                    }
                }
                writeQuickStart?.Invoke();
            }

            AppRun();
        }
        catch (Exception e)
        {
            MessageBox.Show(
                e.ToString(),
                SR.Error,
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
        }
    }
});
t.SetApartmentState(ApartmentState.STA);
t.Start();