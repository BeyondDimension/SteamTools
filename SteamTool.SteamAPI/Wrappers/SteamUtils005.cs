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
    public class SteamUtils005 : NativeWrapper<ISteamUtils005>
    {
        #region GetConnectedUniverse
        [UnmanagedFunctionPointer(CallingConvention.ThisCall)]
        private delegate int NativeGetConnectedUniverse(IntPtr self);

        public int GetConnectedUniverse()
        {
            return this.Call<int, NativeGetConnectedUniverse>(this.Functions.GetConnectedUniverse, this.ObjectAddress);
        }
        #endregion

        #region GetIPCountry
        [UnmanagedFunctionPointer(CallingConvention.ThisCall)]
        private delegate IntPtr NativeGetIPCountry(IntPtr self);

        public string GetIPCountry()
        {
            var result = this.Call<IntPtr, NativeGetIPCountry>(this.Functions.GetIPCountry, this.ObjectAddress);
            return NativeStrings.PointerToString(result);
        }
        #endregion

        #region GetImageSize
        [UnmanagedFunctionPointer(CallingConvention.ThisCall)]
        [return: MarshalAs(UnmanagedType.I1)]
        private delegate bool NativeGetImageSize(IntPtr self, int index, out int width, out int height);

        public bool GetImageSize(int index, out int width, out int height)
        {
            var call = this.GetFunction<NativeGetImageSize>(this.Functions.GetImageSize);
            return call(this.ObjectAddress, index, out width, out height);
        }
        #endregion

        #region GetImageRGBA
        [UnmanagedFunctionPointer(CallingConvention.ThisCall)]
        [return: MarshalAs(UnmanagedType.I1)]
        private delegate bool NativeGetImageRGBA(IntPtr self, int index, byte[] buffer, int length);

        public bool GetImageRGBA(int index, byte[] data)
        {
            if (data == null)
            {
                throw new ArgumentNullException("data");
            }
            var call = this.GetFunction<NativeGetImageRGBA>(this.Functions.GetImageRGBA);
            return call(this.ObjectAddress, index, data, data.Length);
        }
        #endregion

        #region GetAppID
        [UnmanagedFunctionPointer(CallingConvention.ThisCall)]
        private delegate uint NativeGetAppId(IntPtr self);

        public uint GetAppId()
        {
            return this.Call<uint, NativeGetAppId>(this.Functions.GetAppID, this.ObjectAddress);
        }
        #endregion
    }
}
