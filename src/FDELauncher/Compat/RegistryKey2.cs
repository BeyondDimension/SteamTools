// https://newbedev.com/what-are-some-alternatives-to-registrykey-openbasekey-in-net-3-5
#if NET35
using Microsoft.Win32.SafeHandles;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Reflection;
using System.Runtime.InteropServices;

namespace Microsoft.Win32
{
    public static class RegistryKey2
    {
        static readonly Dictionary<RegistryHive, UIntPtr> _hiveKeys = new() {
            { RegistryHive.ClassesRoot, new UIntPtr(0x80000000u) },
            { RegistryHive.CurrentConfig, new UIntPtr(0x80000005u) },
            { RegistryHive.CurrentUser, new UIntPtr(0x80000001u) },
            { RegistryHive.DynData, new UIntPtr(0x80000006u) },
            { RegistryHive.LocalMachine, new UIntPtr(0x80000002u) },
            { RegistryHive.PerformanceData, new UIntPtr(0x80000004u) },
            { RegistryHive.Users, new UIntPtr(0x80000003u) }
        };

        [DllImport("advapi32.dll", CharSet = CharSet.Auto)]
        static extern int RegOpenKeyEx(
          UIntPtr hKey,
          string subKey,
          uint ulOptions,
          uint samDesired,
          out IntPtr hkResult);

        public static RegistryKey OpenBaseKey(RegistryHive hKey, RegistryView view)
        {
            UIntPtr hiveKey = _hiveKeys[hKey];
            if (Environment.OSVersion.Platform == PlatformID.Win32NT && Environment.OSVersion.Version.Major > 5)
            {
                if (view == RegistryView.Default) view = Environment2.Is64BitProcess ? RegistryView.Registry64 : RegistryView.Registry32;
                RegistryAccessMask flags = RegistryAccessMask.QueryValue | RegistryAccessMask.EnumerateSubKeys | RegistryAccessMask.SetValue | RegistryAccessMask.CreateSubKey | (RegistryAccessMask)(int)view;
                int result = RegOpenKeyEx(hiveKey, string.Empty, 0, (uint)flags, out var keyHandlePointer);
                if (result == 0)
                {
                    var safeRegistryHandleType = typeof(SafeHandleZeroOrMinusOneIsInvalid).Assembly.GetType("Microsoft.Win32.SafeHandles.SafeRegistryHandle");
                    var safeRegistryHandleConstructor = safeRegistryHandleType.GetConstructor(BindingFlags.Instance | BindingFlags.NonPublic, null, new[] { typeof(IntPtr), typeof(bool) }, null); // .NET < 4
                    if (safeRegistryHandleConstructor == null)
                        safeRegistryHandleConstructor = safeRegistryHandleType.GetConstructor(BindingFlags.Instance | BindingFlags.Public, null, new[] { typeof(IntPtr), typeof(bool) }, null); // .NET >= 4
                    var keyHandle = safeRegistryHandleConstructor.Invoke(new object[] { keyHandlePointer, true });
                    var net3Constructor = typeof(RegistryKey).GetConstructor(BindingFlags.Instance | BindingFlags.NonPublic, null, new[] { safeRegistryHandleType, typeof(bool) }, null);
                    var net4Constructor = typeof(RegistryKey).GetConstructor(BindingFlags.Instance | BindingFlags.NonPublic, null, new[] { typeof(IntPtr), typeof(bool), typeof(bool), typeof(bool), typeof(bool) }, null);
                    object key;
                    if (net4Constructor != null)
                        key = net4Constructor.Invoke(new object[] { keyHandlePointer, true, false, false, hiveKey == _hiveKeys[RegistryHive.PerformanceData] });
                    else if (net3Constructor != null)
                        key = net3Constructor.Invoke(new object[] { keyHandle, true });
                    else
                    {
                        var keyFromHandleMethod = typeof(RegistryKey).GetMethod("FromHandle", BindingFlags.Static | BindingFlags.Public, null, new[] { safeRegistryHandleType }, null);
                        key = keyFromHandleMethod.Invoke(null, new object[] { keyHandle });
                    }
                    var field = typeof(RegistryKey).GetField("keyName", BindingFlags.Instance | BindingFlags.NonPublic);
                    if (field != null)
                        field.SetValue(key, string.Empty);
                    return (RegistryKey)key;
                }
                else if (result == 2) // The key does not exist.
                    throw new KeyNotFoundException();
                throw new Win32Exception(result);
            }
            throw new PlatformNotSupportedException("The platform or operating system must be Windows XP or later.");
        }

        [Flags]
        enum RegistryAccessMask
        {
            QueryValue = 0x0001,
            SetValue = 0x0002,
            CreateSubKey = 0x0004,
            EnumerateSubKeys = 0x0008,
            Notify = 0x0010,
            CreateLink = 0x0020,
            WoW6432 = 0x0200,
            Wow6464 = 0x0100,
            Write = 0x20006,
            Read = 0x20019,
            Execute = 0x20019,
            AllAccess = 0xF003F
        }
    }

    public enum RegistryView
    {
        Default,
        Registry32 = 512,
        Registry64 = 256,
    }
}
#endif