using System;
using System.IO;
using Microsoft.Win32;
using System.Text;
using System.Runtime.InteropServices;

/*
 Steamworks and NativeWrapper classes provided by Rick - http://gib.me/
*/

namespace Steam4NET
{
    public class Steamworks
    {
        private struct Native
        {

            [DllImport("kernel32.dll", SetLastError = true)]
            internal static extern IntPtr GetProcAddress(IntPtr hModule, string procName);

            [DllImport("kernel32.dll", SetLastError = true)]
            internal static extern IntPtr LoadLibraryEx(string lpszLib, IntPtr hFile, UInt32 dwFlags);

            [DllImport("kernel32.dll", SetLastError = true)]
            internal static extern IntPtr SetDllDirectory(string lpPathName);


            internal const UInt32 LOAD_WITH_ALTERED_SEARCH_PATH = 8;


            [UnmanagedFunctionPointer(CallingConvention.StdCall, CharSet = CharSet.Ansi)]
            internal delegate IntPtr _f(string version);

            [UnmanagedFunctionPointer(CallingConvention.StdCall, CharSet = CharSet.Ansi)]
            internal delegate IntPtr CreateInterface(string version, IntPtr returnCode);

            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            [return: MarshalAs(UnmanagedType.I1)]
            internal delegate bool SteamBGetCallback(int pipe, ref CallbackMsg_t message);

            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            [return: MarshalAs(UnmanagedType.I1)]
            internal delegate bool SteamGetAPICallResult(int hSteamPipe, ulong hSteamAPICall, IntPtr pCallback, int cubCallback, int iCallbackExpected, ref bool pbFailed);

            [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
            [return: MarshalAs(UnmanagedType.I1)]
            internal delegate bool SteamFreeLastCallback(int pipe);

            // helper
            internal static TDelegate GetExportFunction<TDelegate>(IntPtr module, string name) where TDelegate : class
            {
                IntPtr address = Native.GetProcAddress(module, name);

                if (address == IntPtr.Zero)
                    return null;

                return (TDelegate)(object)Marshal.GetDelegateForFunctionPointer(address, typeof(TDelegate));
            }

        }


        private static IntPtr SteamClientHandle = IntPtr.Zero;
        private static IntPtr SteamHandle = IntPtr.Zero;


        /// <summary>
        /// Gets the steam installation path.
        /// </summary>
        /// <returns>A string representing the steam installation directory, or an empty string if steam is not installed</returns>
        public static string GetInstallPath()
        {
            string installPath = "";

            try
            {
                installPath = (string)Registry.GetValue(
                     @"HKEY_CURRENT_USER\SOFTWARE\Valve\Steam",
                     "SteamPath",
                     null);
            }
            catch
            {
            }

            return System.IO.Path.GetFullPath(installPath);
        }

        private static Native.CreateInterface CallCreateInterface;
        /// <summary>
        /// Creates an interface from steamclient.
        /// </summary>
        /// <typeparam name="TClass">The interface type. ex: ISteamClient009</typeparam>
        /// <param name="version">The interface version.</param>
        /// <returns>An instance of an interface object, or null if an error occurred.</returns>
        public static TClass CreateInterface<TClass>() where TClass : InteropHelp.INativeWrapper, new()
        {
            if (CallCreateInterface == null)
                throw new InvalidOperationException("Steam4NET library has not been initialized.");

            IntPtr address = CallCreateInterface(InterfaceVersions.GetInterfaceIdentifier(typeof(TClass)), IntPtr.Zero);

            if (address == IntPtr.Zero)
                return default;

            var rez = new TClass();
            rez.SetupFunctions(address);

            return rez;
        }

        private static Native._f CallCreateSteamInterface;
        /// <summary>
        /// Creates an interface from steam.
        /// </summary>
        /// <typeparam name="TClass">The interface type. ex: ISteam006</typeparam>
        /// <param name="version">The interface version.</param>
        /// <returns>An instance of an interface object, or null if an error occurred.</returns>
        public static TClass CreateSteamInterface<TClass>() where TClass : InteropHelp.INativeWrapper, new()
        {
            if (CallCreateSteamInterface == null)
                throw new InvalidOperationException("Steam4NET library has not been initialized.");

            IntPtr address = CallCreateSteamInterface(InterfaceVersions.GetInterfaceIdentifier(typeof(TClass)));

            if (address == IntPtr.Zero)
                return default;

            var rez = new TClass();
            rez.SetupFunctions(address);

            return rez;
        }

        private static Native.SteamBGetCallback CallSteamBGetCallback;
        /// <summary>
        /// Gets the last callback in steamclient's callback queue.
        /// </summary>
        /// <param name="pipe">The steam pipe.</param>
        /// <param name="message">A reference to a callback object to copy the callback to.</param>
        /// <returns>True if a callback was copied, or false if no callback was waiting, or an error occured.</returns>
        public static bool GetCallback(int pipe, ref CallbackMsg_t message)
        {
            if (CallSteamBGetCallback == null)
                throw new InvalidOperationException("Steam4NET library has not been initialized.");

            try
            {
                return CallSteamBGetCallback(pipe, ref message);
            }
            catch
            {
                message = new CallbackMsg_t();
                return false;
            }
        }

        private static Native.SteamFreeLastCallback CallSteamFreeLastCallback;
        /// <summary>
        /// Frees the last callback in steamclient's callback queue.
        /// </summary>
        /// <param name="pipe">The steam pipe.</param>
        /// <returns>True if the callback was freed; otherwise, false.</returns>
        public static bool FreeLastCallback(int pipe)
        {
            if (CallSteamFreeLastCallback == null)
                throw new InvalidOperationException("Steam4NET library has not been initialized.");

            return CallSteamFreeLastCallback(pipe);
        }

        private static Native.SteamGetAPICallResult CallSteamGetAPICallResult;
        public static bool GetAPICallResult(int hSteamPipe, ulong hSteamAPICall, IntPtr pCallback, int cubCallback, int iCallbackExpected, ref bool pbFailed)
        {
            if (CallSteamGetAPICallResult == null)
                throw new InvalidOperationException("Steam4NET library has not been initialized.");

            return CallSteamGetAPICallResult(hSteamPipe, hSteamAPICall, pCallback, cubCallback, iCallbackExpected, ref pbFailed);
        }

        /// <summary>
        /// Loads the steamclient library. This does not load the steam library. Please use the overload to do so.
        /// </summary>
        /// <returns>A value indicating if the load was successful.</returns>
        public static bool Load()
        {
            return Load(false);
        }
        /// <summary>
        /// Loads the steamclient library and, optionally, the steam library.
        /// </summary>
        /// <param name="steam">if set to <c>true</c> the steam library is also loaded.</param>
        /// <returns>A value indicating if the load was successful.</returns>
        public static bool Load(bool steam)
        {
            if (steam && !LoadSteam())
                return false;

            return LoadSteamClient();
        }

        /// <summary>
        /// Loads the steam library.
        /// </summary>
        /// <returns>A value indicating if the load was successful.</returns>
        public static bool LoadSteam()
        {
            if (SteamHandle != IntPtr.Zero)
                return true;

            string path = GetInstallPath();
            if (!string.IsNullOrEmpty(path))
                Native.SetDllDirectory(path + ";" + Path.Combine(path, "bin"));

            path = Path.Combine(path, "steam.dll");

            IntPtr module = Native.LoadLibraryEx(path, IntPtr.Zero, Native.LOAD_WITH_ALTERED_SEARCH_PATH);

            if (module == IntPtr.Zero)
                return false;

            CallCreateSteamInterface = Native.GetExportFunction<Native._f>(module, "_f");

            if (CallCreateSteamInterface == null)
                return false;

            SteamHandle = module;

            return true;
        }

        /// <summary>
        /// Loads the steamclient library.
        /// </summary>
        /// <returns>A value indicating if the load was successful.</returns>
        public static bool LoadSteamClient()
        {
            if (SteamClientHandle != IntPtr.Zero)
                return true;

            string path = GetInstallPath();

            if (!string.IsNullOrEmpty(path))
                Native.SetDllDirectory(path + ";" + Path.Combine(path, "bin"));
            if (Environment.Is64BitProcess)
                path = Path.Combine(path, "steamclient64.dll");
            else
                path = Path.Combine(path, "steamclient.dll");
            IntPtr module = Native.LoadLibraryEx(path, IntPtr.Zero, Native.LOAD_WITH_ALTERED_SEARCH_PATH);

            if (module == IntPtr.Zero)
                return false;

            CallCreateInterface = Native.GetExportFunction<Native.CreateInterface>(module, "CreateInterface");
            if (CallCreateInterface == null)
                return false;

            CallSteamBGetCallback = Native.GetExportFunction<Native.SteamBGetCallback>(module, "Steam_BGetCallback");
            if (CallSteamBGetCallback == null)
                return false;

            CallSteamFreeLastCallback = Native.GetExportFunction<Native.SteamFreeLastCallback>(module, "Steam_FreeLastCallback");
            if (CallSteamFreeLastCallback == null)
                return false;

            CallSteamGetAPICallResult = Native.GetExportFunction<Native.SteamGetAPICallResult>(module, "Steam_GetAPICallResult");
            if (CallSteamGetAPICallResult == null)
                return false;

            SteamClientHandle = module;

            return true;
        }
    }
}