using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Windows.Forms;

var t = new Thread(() =>
{
    var mainExePath = Path.Combine(AppContext.BaseDirectory, "Steam++.exe");
    if (File.Exists(mainExePath))
    {
        var args_ = string.Join(" ", args);
        var psi = new ProcessStartInfo
        {
            FileName = mainExePath,
            Arguments = args_,
            UseShellExecute = false,
        };
        Process.Start(psi);
    }
    else
    {
        MessageBox.Show("Failed to start, main module not found.", nameof(MessageBoxIcon.Error));
    }
    Environment.Exit(0);
});
t.SetApartmentState(ApartmentState.STA);
t.Start();