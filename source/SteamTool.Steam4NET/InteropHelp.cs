using System;
using System.Text;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Reflection;

namespace Steam4NET
{
    public class InteropHelp
    {

        /// <summary>
        /// Decodes ANSI encoded return string to UTF-8
        /// </summary>
        public static string DecodeANSIReturn( string buffer )
        {
            return Encoding.UTF8.GetString( Encoding.Default.GetBytes( buffer ) );
        }

        /// <summary>
        /// Casts an interface from a pointer to a object representing the interface.
        /// </summary>
        /// <typeparam name="TClass">The interface type. ex: ISteamUser013</typeparam>
        /// <param name="address">The address of the interface.</param>
        /// <returns>An instance of an interface object, or null if an error occurred.</returns>
        public static TClass CastInterface<TClass>(IntPtr address)
            where TClass : INativeWrapper, new()
        {
            if (address == IntPtr.Zero)
                return default(TClass);

            var rez = new TClass();
            rez.SetupFunctions(address);
            return rez;
        }

        public interface INativeWrapper
        {
            void SetupFunctions(IntPtr objectAddress);
        }

        public abstract class NativeWrapper<TNativeFunctions> : INativeWrapper
        {
            protected IntPtr ObjectAddress;

            public IntPtr Interface
            {
                get { return ObjectAddress; }
            }

            protected TNativeFunctions Functions;

            public override string ToString()
            {
                return string.Format(
                    "Steam Interface<{0}> #{1:X8}",
                    typeof(TNativeFunctions),
                    this.ObjectAddress.ToInt32());
            }


            public void SetupFunctions(IntPtr objectAddress)
            {
                this.ObjectAddress = objectAddress;

                IntPtr vtableptr = Marshal.ReadIntPtr(this.ObjectAddress);

                this.Functions = (TNativeFunctions)Marshal.PtrToStructure(
                    vtableptr, typeof(TNativeFunctions));
            }

            private Dictionary<IntPtr, Delegate> FunctionCache = new Dictionary<IntPtr, Delegate>();

            protected Delegate GetDelegate<TDelegate>(IntPtr pointer)
            {
                Delegate function;

                if (this.FunctionCache.ContainsKey(pointer) == false)
                {
                    function = Marshal.GetDelegateForFunctionPointer(pointer, typeof(TDelegate));
                    this.FunctionCache[pointer] = function;
                }
                else
                {
                    function = this.FunctionCache[pointer];
                }

                return function;
            }

            protected TDelegate GetFunction<TDelegate>(IntPtr pointer)
                where TDelegate : class
            {
                return (TDelegate)((object)this.GetDelegate<TDelegate>(pointer));
            }

            protected void Call<TDelegate>(IntPtr pointer, params object[] args)
            {
                this.GetDelegate<TDelegate>(pointer).DynamicInvoke(args);
            }

            protected TReturn Call<TReturn, TDelegate>(IntPtr pointer, params object[] args)
            {
                return (TReturn)this.GetDelegate<TDelegate>(pointer).DynamicInvoke(args);
            }
        }

        internal class BitVector64
        {
            private UInt64 data;

            public BitVector64()
            {
            }
            public BitVector64(UInt64 value)
            {
                data = value;
            }

            public UInt64 Data
            {
                get { return data; }
                set { data = value; }
            }

            public UInt64 this[uint bitoffset, UInt64 valuemask]
            {
                get
                {
                    return (data >> (ushort)bitoffset) & valuemask;
                }
                set
                {
                    data = (data & ~(valuemask << (ushort)bitoffset)) | ((value & valuemask) << (ushort)bitoffset);
                }
            }
        }

        [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
        internal class InterfaceVersionAttribute : System.Attribute
        {
            public string Identifier { get; set; }

            public InterfaceVersionAttribute(string versionIdentifier)
            {
                Identifier = versionIdentifier;
            }
        }

        [AttributeUsage(AttributeTargets.Struct, AllowMultiple = false)]
        internal class CallbackIdentityAttribute : System.Attribute
        {
            public int Identity { get; set; }

            public CallbackIdentityAttribute(int callbackNum)
            {
                Identity = callbackNum;
            }
        }
    }
}
