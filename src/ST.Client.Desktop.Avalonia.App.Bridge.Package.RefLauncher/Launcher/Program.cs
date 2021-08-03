using System;
using System.Diagnostics;
using System.IO;

try
{
    Process.Start(new ProcessStartInfo()
    {
        FileName = Path.Combine(AppContext.BaseDirectory, "..", "Steam++.exe"),
        Arguments = string.Join(" ", args),
        UseShellExecute = false,
    });
}
catch (FileNotFoundException)
{

}