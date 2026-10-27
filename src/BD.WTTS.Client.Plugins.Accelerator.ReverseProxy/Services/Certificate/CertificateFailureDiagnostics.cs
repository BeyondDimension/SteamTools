using System.Runtime.InteropServices;
using System.Security.Cryptography;
using Microsoft.Win32;

namespace BD.WTTS.Services;

internal enum CertificateFailureReason
{
    CertificateFailure,
    KeyIsolationDisabled,
    KeyIsolationStopped,
}

internal readonly record struct KeyIsolationSnapshot(int? StartupType, int? State);

internal static class CertificateFailureDiagnostics
{
    internal static CertificateFailureReason Classify(Exception exception, bool isWindows, int? startupType, int? state)
    {
        var isCryptographicFailure = false;
        for (var current = exception; current != null; current = current.InnerException)
        {
            if (current is CryptographicException)
            {
                isCryptographicFailure = true;
                break;
            }
        }

        if (!isWindows || !isCryptographicFailure)
            return CertificateFailureReason.CertificateFailure;
        if (startupType == 4)
            return CertificateFailureReason.KeyIsolationDisabled;
        if (state == 1)
            return CertificateFailureReason.KeyIsolationStopped;
        return CertificateFailureReason.CertificateFailure;
    }

    internal static CertificateFailureReason ClassifyWithPreviousState(Exception exception, bool isWindows,
        int? previousStartupType, int? previousState, int? currentStartupType, int? currentState)
    {
        var current = Classify(exception, isWindows, currentStartupType, currentState);
        if (current == CertificateFailureReason.KeyIsolationDisabled)
            return current;
        var previous = Classify(exception, isWindows, previousStartupType, previousState);
        if (previous == CertificateFailureReason.KeyIsolationDisabled)
            return previous;
        return current != CertificateFailureReason.CertificateFailure ? current : previous;
    }

    internal static KeyIsolationSnapshot Capture()
    {
        int? startupType = null;
        int? state = null;
        if (OperatingSystem.IsWindows())
        {
            try
            {
                using var key = Registry.LocalMachine.OpenSubKey(@"SYSTEM\CurrentControlSet\Services\KeyIso");
                startupType = key?.GetValue("Start") as int?;
            }
            catch (Exception)
            {
                // A missing service or inaccessible configuration is not evidence that KeyIso is disabled.
            }
            try
            {
                state = GetServiceState();
            }
            catch (Exception)
            {
                // Service status may be unavailable without sufficient access.
            }
        }
        return new(startupType, state);
    }

    internal static CertificateFailureReason Diagnose(Exception exception, KeyIsolationSnapshot previous)
    {
        var current = Capture();
        return ClassifyWithPreviousState(exception, OperatingSystem.IsWindows(),
            previous.StartupType, previous.State, current.StartupType, current.State);
    }

    static int? GetServiceState()
    {
        var manager = OpenSCManager(null, null, 0x0001);
        if (manager == IntPtr.Zero)
            return null;
        try
        {
            var service = OpenService(manager, "KeyIso", 0x0004);
            if (service == IntPtr.Zero)
                return null;
            try
            {
                return QueryServiceStatus(service, out var status) ? (int)status.CurrentState : null;
            }
            finally { CloseServiceHandle(service); }
        }
        finally { CloseServiceHandle(manager); }
    }

    [StructLayout(LayoutKind.Sequential)]
    struct ServiceStatus
    {
        public uint ServiceType, CurrentState, ControlsAccepted, Win32ExitCode, ServiceSpecificExitCode, CheckPoint, WaitHint;
    }

    [DllImport("advapi32.dll", EntryPoint = "OpenSCManagerW", CharSet = CharSet.Unicode, SetLastError = true)]
    static extern IntPtr OpenSCManager(string? machineName, string? databaseName, uint desiredAccess);

    [DllImport("advapi32.dll", EntryPoint = "OpenServiceW", CharSet = CharSet.Unicode, SetLastError = true)]
    static extern IntPtr OpenService(IntPtr manager, string serviceName, uint desiredAccess);

    [DllImport("advapi32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    static extern bool QueryServiceStatus(IntPtr service, out ServiceStatus status);

    [DllImport("advapi32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    static extern bool CloseServiceHandle(IntPtr handle);
}
