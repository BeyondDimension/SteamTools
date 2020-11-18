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
using System.Text;
using Microsoft.Win32.SafeHandles;

namespace SAM.API
{
    internal class NativeStrings
    {
        public sealed class StringHandle : SafeHandleZeroOrMinusOneIsInvalid
        {
            internal StringHandle(IntPtr preexistingHandle, bool ownsHandle)
                : base(ownsHandle)
            {
                this.SetHandle(preexistingHandle);
            }

            public IntPtr Handle
            {
                get { return this.handle; }
            }

            protected override bool ReleaseHandle()
            {
                if (handle != IntPtr.Zero)
                {
                    Marshal.FreeHGlobal(handle);
                    handle = IntPtr.Zero;
                    return true;
                }

                return false;
            }
        }

        public static unsafe StringHandle StringToStringHandle(string value)
        {
            if (value == null)
            {
                return new StringHandle(IntPtr.Zero, true);
            }

            var bytes = Encoding.UTF8.GetBytes(value);
            var length = bytes.Length;

            var p = Marshal.AllocHGlobal(length + 1);
            Marshal.Copy(bytes, 0, p, bytes.Length);
            ((byte*)p)[length] = 0;
            return new StringHandle(p, true);
        }

        public static unsafe string PointerToString(sbyte* bytes)
        {
            if (bytes == null)
            {
                return null;
            }

            int running = 0;

            var b = bytes;
            if (*b == 0)
            {
                return string.Empty;
            }

            while ((*b++) != 0)
            {
                running++;
            }

            return new string(bytes, 0, running, Encoding.UTF8);
        }

        public static unsafe string PointerToString(byte* bytes)
        {
            return PointerToString((sbyte*)bytes);
        }

        public static unsafe string PointerToString(IntPtr nativeData)
        {
            return PointerToString((sbyte*)nativeData.ToPointer());
        }

        public static unsafe string PointerToString(sbyte* bytes, int length)
        {
            if (bytes == null)
            {
                return null;
            }

            int running = 0;

            var b = bytes;
            if (length == 0 || *b == 0)
            {
                return string.Empty;
            }

            while ((*b++) != 0 &&
                   running < length)
            {
                running++;
            }

            return new string(bytes, 0, running, Encoding.UTF8);
        }

        public static unsafe string PointerToString(byte* bytes, int length)
        {
            return PointerToString((sbyte*)bytes, length);
        }

        public static unsafe string PointerToString(IntPtr nativeData, int length)
        {
            return PointerToString((sbyte*)nativeData.ToPointer(), length);
        }
    }
}
