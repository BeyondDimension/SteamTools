using System;
using System.Diagnostics;
using System.IO;
using System.Threading;

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
#if DEBUG
        LinkHelpers.Handle();
        return;
#else
        MessageBox.Show("Failed to start, main module not found.", nameof(MessageBoxIcon.Error));
#endif
    }
});
t.SetApartmentState(ApartmentState.STA);
t.Start();