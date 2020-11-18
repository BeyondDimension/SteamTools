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
    public class SteamUserStats007 : NativeWrapper<ISteamUserStats007>
    {
        #region RequestCurrentStats
        [UnmanagedFunctionPointer(CallingConvention.ThisCall)]
        [return: MarshalAs(UnmanagedType.I1)]
        private delegate bool NativeRequestCurrentStats(IntPtr self);

        public bool RequestCurrentStats()
        {
            return this.Call<bool, NativeRequestCurrentStats>(this.Functions.RequestCurrentStats, this.ObjectAddress);
        }
        #endregion

        #region GetStatValue (int)
        [UnmanagedFunctionPointer(CallingConvention.ThisCall)]
        [return: MarshalAs(UnmanagedType.I1)]
        private delegate bool NativeGetStatInt(IntPtr self, IntPtr name, out int data);

        public bool GetStatValue(string name, out int value)
        {
            using (var nativeName = NativeStrings.StringToStringHandle(name))
            {
                var call = this.GetFunction<NativeGetStatInt>(this.Functions.GetStatInteger);
                return call(this.ObjectAddress, nativeName.Handle, out value);
            }
        }
        #endregion

        #region GetStatValue (float)
        [UnmanagedFunctionPointer(CallingConvention.ThisCall)]
        [return: MarshalAs(UnmanagedType.I1)]
        private delegate bool NativeGetStatFloat(IntPtr self, IntPtr name, out float data);

        public bool GetStatValue(string name, out float value)
        {
            using (var nativeName = NativeStrings.StringToStringHandle(name))
            {
                var call = this.GetFunction<NativeGetStatFloat>(this.Functions.GetStatFloat);
                return call(this.ObjectAddress, nativeName.Handle, out value);
            }
        }
        #endregion

        #region SetStatValue (int)
        [UnmanagedFunctionPointer(CallingConvention.ThisCall)]
        [return: MarshalAs(UnmanagedType.I1)]
        private delegate bool NativeSetStatInt(IntPtr self, IntPtr name, int data);

        public bool SetStatValue(string name, int value)
        {
            using (var nativeName = NativeStrings.StringToStringHandle(name))
            {
                return this.Call<bool, NativeSetStatInt>(
                    this.Functions.SetStatInteger,
                    this.ObjectAddress,
                    nativeName.Handle,
                    value);
            }
        }
        #endregion

        #region SetStatValue (float)
        [UnmanagedFunctionPointer(CallingConvention.ThisCall)]
        [return: MarshalAs(UnmanagedType.I1)]
        private delegate bool NativeSetStatFloat(IntPtr self, IntPtr name, float data);

        public bool SetStatValue(string name, float value)
        {
            using (var nativeName = NativeStrings.StringToStringHandle(name))
            {
                return this.Call<bool, NativeSetStatFloat>(
                    this.Functions.SetStatFloat,
                    this.ObjectAddress,
                    nativeName.Handle,
                    value);
            }
        }
        #endregion

        #region GetAchievement
        [UnmanagedFunctionPointer(CallingConvention.ThisCall)]
        [return: MarshalAs(UnmanagedType.I1)]
        private delegate bool NativeGetAchievement(
            IntPtr self,
            IntPtr name,
            [MarshalAs(UnmanagedType.I1)] out bool isAchieved);

        public bool GetAchievementState(string name, out bool isAchieved)
        {
            using (var nativeName = NativeStrings.StringToStringHandle(name))
            {
                var call = this.GetFunction<NativeGetAchievement>(this.Functions.GetAchievement);
                return call(this.ObjectAddress, nativeName.Handle, out isAchieved);
            }
        }
        #endregion

        #region SetAchievementState
        [UnmanagedFunctionPointer(CallingConvention.ThisCall)]
        [return: MarshalAs(UnmanagedType.I1)]
        private delegate bool NativeSetAchievement(IntPtr self, IntPtr name);

        [UnmanagedFunctionPointer(CallingConvention.ThisCall)]
        [return: MarshalAs(UnmanagedType.I1)]
        private delegate bool NativeClearAchievement(IntPtr self, IntPtr name);

        public bool SetAchievement(string name, bool state)
        {
            using (var nativeName = NativeStrings.StringToStringHandle(name))
            {
                if (state == false)
                {
                    return this.Call<bool, NativeClearAchievement>(
                        this.Functions.ClearAchievement,
                        this.ObjectAddress,
                        nativeName.Handle);
                }

                return this.Call<bool, NativeSetAchievement>(
                    this.Functions.SetAchievement,
                    this.ObjectAddress,
                    nativeName.Handle);
            }
        }
        #endregion

        #region StoreStats
        [UnmanagedFunctionPointer(CallingConvention.ThisCall)]
        [return: MarshalAs(UnmanagedType.I1)]
        private delegate bool NativeStoreStats(IntPtr self);

        public bool StoreStats()
        {
            return this.Call<bool, NativeStoreStats>(this.Functions.StoreStats, this.ObjectAddress);
        }
        #endregion

        #region GetAchievementIcon
        [UnmanagedFunctionPointer(CallingConvention.ThisCall)]
        private delegate int NativeGetAchievementIcon(IntPtr self, IntPtr name);

        public int GetAchievementIcon(string name)
        {
            using (var nativeName = NativeStrings.StringToStringHandle(name))
            {
                return this.Call<int, NativeGetAchievementIcon>(
                    this.Functions.GetAchievementIcon,
                    this.ObjectAddress,
                    nativeName.Handle);
            }
        }
        #endregion

        #region GetAchievementDisplayAttribute
        [UnmanagedFunctionPointer(CallingConvention.ThisCall)]
        private delegate IntPtr NativeGetAchievementDisplayAttribute(IntPtr self, IntPtr name, IntPtr key);

        public string GetAchievementDisplayAttribute(string name, string key)
        {
            using (var nativeName = NativeStrings.StringToStringHandle(name))
            using (var nativeKey = NativeStrings.StringToStringHandle(key))
            {
                var result = this.Call<IntPtr, NativeGetAchievementDisplayAttribute>(
                    this.Functions.GetAchievementDisplayAttribute,
                    this.ObjectAddress,
                    nativeName.Handle,
                    nativeKey.Handle);
                return NativeStrings.PointerToString(result);
            }
        }
        #endregion

        #region ResetAllStats
        [UnmanagedFunctionPointer(CallingConvention.ThisCall)]
        [return: MarshalAs(UnmanagedType.I1)]
        private delegate bool NativeResetAllStats(IntPtr self, [MarshalAs(UnmanagedType.I1)] bool achievementsToo);

        public bool ResetAllStats(bool achievementsToo)
        {
            return this.Call<bool, NativeResetAllStats>(
                this.Functions.ResetAllStats,
                this.ObjectAddress,
                achievementsToo);
        }
        #endregion
    }
}
