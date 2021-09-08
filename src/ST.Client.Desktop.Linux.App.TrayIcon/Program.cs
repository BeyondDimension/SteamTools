using System;
using System.Application.UI;
using System.Linq;
using System.Windows;
using GtkApplication = Gtk.Application;

if (!OperatingSystem.IsLinux()) return;
try
{
    var handle = args.FirstOrDefault();
    if (string.IsNullOrWhiteSpace(handle)) return;

    // https://www.mono-project.com/docs/gui/gtksharp/widgets/notification-icon/
    // Initialize GTK#
    GtkApplication.Init();

    DI.Init(services =>
    {
        services.AddNotifyIcon();
    });

    NotifyIconHelper.StartPipeClient(handle);

    (var notifyIcon, var menuItemDisposable) = NotifyIconHelper.Init(NotifyIconHelper.GetIcon);
    notifyIcon.Click += (_, _) =>
    {
        NotifyIconHelper.Client.Append(NotifyIconHelper.PipeCore.CommandNotifyIconClick);
    };

    GtkApplication.Run();

    menuItemDisposable?.Dispose();
    notifyIcon.Dispose();
    NotifyIconHelper.StopPipeClient();
}
catch (Exception e)
{
    Console.WriteLine(e);
}