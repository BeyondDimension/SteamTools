/* Copyright (c) 2019 Rick (rick 'at' gibbed 'dot' us)
 * 
 * This software is provided 'as-is', without any express or implied
 * warranty. In no event will the authors be held liable for any damages
 * arising from the use of this software.
 * 
 * Permission is granted to anyone to use this software for any purpose,
 * including commercial applications, and to alter it and redistribute it
 * freely, subject to the following restrictions:
 * 
 * 1. The origin of this software must not be misrepresented; you must not
 *    claim that you wrote the original software. If you use this software
 *    in a product, an acknowledgment in the product documentation would
 *    be appreciated but is not required.
 * 
 * 2. Altered source versions must be plainly marked as such, and must not
 *    be misrepresented as being the original software.
 * 
 * 3. This notice may not be removed or altered from any source
 *    distribution.
 */

using System;
using System.Runtime.InteropServices;
using SAM.API.Interfaces;

namespace SAM.API.Wrappers
{
    public class SteamClient018 : NativeWrapper<ISteamClient018>
    {
        #region CreateSteamPipe
        [UnmanagedFunctionPointer(CallingConvention.ThisCall)]
        private delegate int NativeCreateSteamPipe(IntPtr self);

        public int CreateSteamPipe()
        {
            return this.Call<int, NativeCreateSteamPipe>(this.Functions.CreateSteamPipe, this.ObjectAddress);
        }
        #endregion

        #region ReleaseSteamPipe
        [UnmanagedFunctionPointer(CallingConvention.ThisCall)]
        [return: MarshalAs(UnmanagedType.I1)]
        private delegate bool NativeReleaseSteamPipe(IntPtr self, int pipe);

        public bool ReleaseSteamPipe(int pipe)
        {
            return this.Call<bool, NativeReleaseSteamPipe>(this.Functions.ReleaseSteamPipe, this.ObjectAddress, pipe);
        }
        #endregion

        #region CreateLocalUser
        [UnmanagedFunctionPointer(CallingConvention.ThisCall)]
        private delegate int NativeCreateLocalUser(IntPtr self, ref int pipe, Types.AccountType type);

        public int CreateLocalUser(ref int pipe, Types.AccountType type)
        {
            var call = this.GetFunction<NativeCreateLocalUser>(this.Functions.CreateLocalUser);
            return call(this.ObjectAddress, ref pipe, type);
        }
        #endregion

        #region ConnectToGlobalUser
        [UnmanagedFunctionPointer(CallingConvention.ThisCall)]
        private delegate int NativeConnectToGlobalUser(IntPtr self, int pipe);

        public int ConnectToGlobalUser(int pipe)
        {
            return this.Call<int, NativeConnectToGlobalUser>(
                this.Functions.ConnectToGlobalUser,
                this.ObjectAddress,
                pipe);
        }
        #endregion

        #region ReleaseUser
        [UnmanagedFunctionPointer(CallingConvention.ThisCall)]
        private delegate void NativeReleaseUser(IntPtr self, int pipe, int user);

        public void ReleaseUser(int pipe, int user)
        {
            this.Call<NativeReleaseUser>(this.Functions.ReleaseUser, this.ObjectAddress, pipe, user);
        }
        #endregion

        #region SetLocalIPBinding
        [UnmanagedFunctionPointer(CallingConvention.ThisCall)]
        private delegate void NativeSetLocalIPBinding(IntPtr self, uint host, ushort port);

        public void SetLocalIPBinding(uint host, ushort port)
        {
            this.Call<NativeSetLocalIPBinding>(this.Functions.SetLocalIPBinding, this.ObjectAddress, host, port);
        }
        #endregion

        #region GetISteamUser
        [UnmanagedFunctionPointer(CallingConvention.ThisCall)]
        private delegate IntPtr NativeGetISteamUser(IntPtr self, int user, int pipe, IntPtr version);

        private TClass GetISteamUser<TClass>(int user, int pipe, string version)
            where TClass : INativeWrapper, new()
        {
            using (var nativeVersion = NativeStrings.StringToStringHandle(version))
            {
                IntPtr address = this.Call<IntPtr, NativeGetISteamUser>(
                    this.Functions.GetISteamUser,
                    this.ObjectAddress,
                    user,
                    pipe,
                    nativeVersion.Handle);
                var result = new TClass();
                result.SetupFunctions(address);
                return result;
            }
        }
        #endregion

        #region GetSteamUser012
        public SteamUser019 GetSteamUser012(int user, int pipe)
        {
            return this.GetISteamUser<SteamUser019>(user, pipe, "SteamUser019");
        }
        #endregion

        #region GetISteamUserStats
        [UnmanagedFunctionPointer(CallingConvention.ThisCall)]
        private delegate IntPtr NativeGetISteamUserStats(IntPtr self, int user, int pipe, IntPtr version);

        private TClass GetISteamUserStats<TClass>(int user, int pipe, string version)
            where TClass : INativeWrapper, new()
        {
            using (var nativeVersion = NativeStrings.StringToStringHandle(version))
            {
                IntPtr address = this.Call<IntPtr, NativeGetISteamUserStats>(
                    this.Functions.GetISteamUserStats,
                    this.ObjectAddress,
                    user,
                    pipe,
                    nativeVersion.Handle);
                var result = new TClass();
                result.SetupFunctions(address);
                return result;
            }
        }
        #endregion

        #region GetSteamUserStats007
        public SteamUserStats007 GetSteamUserStats006(int user, int pipe)
        {
            return this.GetISteamUserStats<SteamUserStats007>(user, pipe, "STEAMUSERSTATS_INTERFACE_VERSION011");
        }
        #endregion

        #region GetISteamUtils
        [UnmanagedFunctionPointer(CallingConvention.ThisCall)]
        private delegate IntPtr NativeGetISteamUtils(IntPtr self, int pipe, IntPtr version);

        public TClass GetISteamUtils<TClass>(int pipe, string version)
            where TClass : INativeWrapper, new()
        {
            using (var nativeVersion = NativeStrings.StringToStringHandle(version))
            {
                IntPtr address = this.Call<IntPtr, NativeGetISteamUtils>(
                    this.Functions.GetISteamUtils,
                    this.ObjectAddress,
                    pipe,
                    nativeVersion.Handle);
                var result = new TClass();
                result.SetupFunctions(address);
                return result;
            }
        }
        #endregion

        #region GetSteamUtils004
        public SteamUtils009 GetSteamUtils009(int pipe)
        {
            return this.GetISteamUtils<SteamUtils009>(pipe, "SteamUtils009");
        }
        #endregion

        #region GetISteamApps
        private delegate IntPtr NativeGetISteamApps(int user, int pipe, IntPtr version);

        private TClass GetISteamApps<TClass>(int user, int pipe, string version)
            where TClass : INativeWrapper, new()
        {
            using (var nativeVersion = NativeStrings.StringToStringHandle(version))
            {
                IntPtr address = this.Call<IntPtr, NativeGetISteamApps>(
                    this.Functions.GetISteamApps,
                    user,
                    pipe,
                    nativeVersion.Handle);
                var result = new TClass();
                result.SetupFunctions(address);
                return result;
            }
        }
        #endregion

        #region GetSteamApps001
        public SteamApps001 GetSteamApps001(int user, int pipe)
        {
            return this.GetISteamApps<SteamApps001>(user, pipe, "STEAMAPPS_INTERFACE_VERSION001");
        }
        #endregion

        #region GetSteamApps008
        public SteamApps008 GetSteamApps008(int user, int pipe)
        {
            return this.GetISteamApps<SteamApps008>(user, pipe, "STEAMAPPS_INTERFACE_VERSION008");
        }
        #endregion
    }
}
