/*
 * Copyright (C) 2015	 Colin Mackie.
 * This software is distributed under the terms of the GNU General Public License.
 *
 * This program is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with this program.  If not, see <http://www.gnu.org/licenses/>.
 */

using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Reflection;
using System.Runtime.ConstrainedExecution;
using System.Runtime.InteropServices;
using System.Runtime.CompilerServices;
using System.Text;
using System.Security.Cryptography;
using System.Resources;
using System.Threading;

namespace WinAuth
{
	/// <summary>
	/// Store the current YubiKey data and seed from original decryption. This is so
	/// we can update the config file without requiring the user key press (if enabled)
	/// from the yubi. Although DP is used, pretty pointless since it is put into managed
	/// memory anyway.
	/// </summary>
	public class YubikeyData
	{
		/// <summary>
		/// Length of key
		/// </summary>
		public int Length;

		/// <summary>
		/// Random seed
		/// </summary>
		public string Seed;

		/// <summary>
		/// Protected data block
		/// </summary>
		private byte[] _data;

		/// <summary>
		/// Get/set the protected data into a managed array
		/// </summary>
		public byte[] Data
		{
			get
			{
				if (Length == 0)
				{
					return null;
				}

				byte[] b = new byte[_data.Length];
				Array.Copy(_data, b, _data.Length);
				return ProtectedData.Unprotect(b, null, DataProtectionScope.CurrentUser);
			}
			set
			{
				if (value == null)
				{
					_data = null;
					Length = 0;
				}
				else
				{
					Length = value.Length;
					byte[] b = new byte[value.Length];
					Array.Copy(value, b, value.Length);
					_data = ProtectedData.Protect(b, null, DataProtectionScope.CurrentUser);
				}
			}
		}
	}

	/// <summary>
	/// Class wrapping API around pinvoke YubiKey DLL
	/// </summary>
	public class YubiKey : IDisposable
	{
		/// <summary>
		/// USB Device Vendor and Product IDs for YubiKeys
		/// </summary>
		public const int VENDOR_ID = 0x1050;	/* Global vendor ID */
		public const int YUBIKEY_PID = 0x0010;	/* Yubikey (version 1 and 2) */
		public const int NEO_OTP_PID = 0x0110;	/* Yubikey NEO - OTP only */
		public const int NEO_OTP_CCID_PID = 0x0111;	/* Yubikey NEO - OTP and CCID */
		public const int NEO_CCID_PID = 0x0112;	/* Yubikey NEO - CCID only */
		public const int NEO_U2F_PID = 0x0113;	/* Yubikey NEO - U2F only */
		public const int NEO_OTP_U2F_PID = 0x0114;	/* Yubikey NEO - OTP and U2F */
		public const int NEO_U2F_CCID_PID = 0x0115;	/* Yubikey NEO - U2F and CCID */
		public const int NEO_OTP_U2F_CCID_PID = 0x0116;	/* Yubikey NEO - OTP, U2F and CCID */
		public const int YK4_OTP_PID = 0x0401;	/* Yubikey 4 - OTP only */
		public const int YK4_U2F_PID = 0x0402;	/* Yubikey 4 - U2F only */
		public const int YK4_OTP_U2F_PID = 0x0403;	/* Yubikey 4 - OTP and U2F */
		public const int YK4_CCID_PID = 0x0404;	/* Yubikey 4 - CCID only */
		public const int YK4_OTP_CCID_PID = 0x0405;	/* Yubikey 4 - OTP and CCID */
		public const int YK4_U2F_CCID_PID = 0x0406;	/* Yubikey 4 - U2F and CCID */
		public const int YK4_OTP_U2F_CCID_PID = 0x0407;	/* Yubikey 4 - OTP, U2F and CCID */
		public const int PLUS_U2F_OTP_PID = 0x0410;	/* Yubikey plus - OTP+U2F */

		/// <summary>
		/// Array of valid product IDs
		/// </summary>
		public static int[] DEVICE_IDS = new int[] {
			YUBIKEY_PID,
			NEO_OTP_PID,
			NEO_OTP_CCID_PID,
			NEO_CCID_PID,
			NEO_U2F_PID,
			NEO_OTP_U2F_PID,
			NEO_U2F_CCID_PID,
			NEO_OTP_U2F_CCID_PID,
			YK4_OTP_PID,
			YK4_U2F_PID,
			YK4_OTP_U2F_PID,
			YK4_CCID_PID,
			YK4_OTP_CCID_PID,
			YK4_U2F_CCID_PID,
			YK4_OTP_U2F_CCID_PID,
			PLUS_U2F_OTP_PID
		};

		/// <summary>
		/// USDB device GUID for keyboard
		/// </summary>
		public static Guid GUID_DEVINTERFACE_KEYBOARD = new Guid("884b96c3-56ef-11d1-bc8c-00a0c91405dd");

		/// <summary>
		/// Name of AppData folder
		/// </summary>
		public const string APPDATAFOLDER = "WinAuth";

		/// <summary>
		/// Name of our native x86 YubiKey DLL
		/// </summary>
		private const string YUBI_LIBRARY_NAME_X86 = "WinAuth.YubiKey.x86.dll";

		/// <summary>
		/// Name of our native x64 YubiKey DLL
		/// </summary>
		private const string YUBI_LIBRARY_NAME_X64 = "WinAuth.YubiKey.x64.dll";

		/// <summary>
		/// Fix Status stuct returned from Yubkey DLLs
		/// </summary>
		public struct STATUS
		{
			public byte VersionMajor;	/* Firmware version information */
			public byte VersionMinor;
			public byte VersionBuild;
			public byte PgmSeq;		/* Programming sequence number. 0 if no valid configuration */
			public UInt16 TouchLevel;	/* Level from touch detector */
		};
		public struct INFO
		{
			public STATUS Status;
			public UInt32 Serial;
			public UInt32 Pid;
			public string Error;
		};

		/// <summary>
		/// Handle to loaded pinvoke library
		/// </summary>
		private IntPtr _library;

		/// <summary>
		/// Path of file for native library
		/// </summary>
		private string _libraryPath;

		/// <summary>
		/// Current YubiKey info
		/// </summary>
		public INFO Info { get; set; }

		/// <summary>
		/// Native calls
		/// </summary>
		[DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
		private static extern IntPtr LoadLibrary(string libname);
		[DllImport("kernel32.dll", CharSet = CharSet.Auto)]
		[return: MarshalAs(UnmanagedType.Bool)]
		private static extern bool FreeLibrary(IntPtr hModule);
		[DllImport("kernel32.dll", CharSet = CharSet.Ansi)]
		private static extern IntPtr GetProcAddress(IntPtr hModule, string lpProcName);
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		delegate int LastErrorDelegate();
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		delegate bool IsInsertedDelegate();
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		delegate int GetInfoDelegate([MarshalAs(UnmanagedType.Struct)] out INFO status);
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		delegate int SetChallengeResponseDelegate(
			[MarshalAs(UnmanagedType.U4)] int slot,
			byte[] secret,
			[MarshalAs(UnmanagedType.U4)] int keysize,
			[MarshalAs(UnmanagedType.Bool)] bool userpress,
			byte[] access_code);
		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		delegate int ChallengeResponseDelegate(
			[MarshalAs(UnmanagedType.U4)] int slot,
			[MarshalAs(UnmanagedType.Bool)] bool may_block,
			byte[] challenge,
			[MarshalAs(UnmanagedType.U4)] int challenge_len,
			byte[] response,
			[MarshalAs(UnmanagedType.U4)] int response_len);

		/// <summary>
		/// Create the YubiKey instance
		/// </summary>
		protected YubiKey()
		{
			YubiData = new YubikeyData();
		}

		/// <summary>
		/// Finalizer
		/// </summary>
		~YubiKey()
		{
			Dispose(false);
		}

		/// <summary>
		/// Dispose this object
		/// </summary>
		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		/// <summary>
		/// Dispose any resources
		/// </summary>
		/// <param name="disposing">called via Dispose</param>
		protected virtual void Dispose(bool disposing)
		{
			if (disposing)
			{
				// free managed resources
			}

			// free native resources
			if (_library != IntPtr.Zero)
			{
				FreeLibrary(_library);
				_library = IntPtr.Zero;
			}
			// delete temp library file
			if (string.IsNullOrEmpty(_libraryPath) == false && File.Exists(_libraryPath) == true)
			{
				try
				{
					File.Delete(_libraryPath);
				}
				catch (Exception) { }
				_libraryPath = null;
			}
		}

		/// <summary>
		/// Mutex for the init method
		/// </summary>
		private object _initLock = new object();

		/// <summary>
		/// Stored data for Yubikey
		/// </summary>
		public YubikeyData YubiData { get; set; }

		/// <summary>
		/// Create a new Yubikey instance and initialise
		/// </summary>
		/// <param name="waitms"></param>
		/// <returns></returns>
		public static YubiKey CreateInstance()
		{
			YubiKey instance = new YubiKey();

			instance.Init();

			return instance;
		}

		/// <summary>
		/// Initialise the Yubikey and get its status
		/// </summary>
		/// <param name="waitms"></param>
		public void Init()
		{
			lock (_initLock)
			{
				if (Info.Status.VersionMajor != 0)
				{
					return;
				}

				// load library and load info
				LoadLibrary();
				INFO info = new INFO();
				GetInfoDelegate f = GetFunction<GetInfoDelegate>("GetInfo");
				int ret = f(out info);
				if (ret > 1)
				{
					info.Error = string.Format("Error {0}", ret);
				}
				Info = info;
			}
		}

		/// <summary>
		/// Load the 32 or 64 bit native YubiKey DLL depending on current platform, or download from winauth servers
		/// </summary>
		/// <param name="downloadIfNeeded">option to download DLL if required</param>
		private void LoadLibrary()
		{
			if (_library != IntPtr.Zero)
			{
				return;
			}

			// load the library
			var libraryname = (IntPtr.Size == 4 ? YUBI_LIBRARY_NAME_X86 : YUBI_LIBRARY_NAME_X64);

			// create temp file
			_libraryPath = Path.GetTempFileName();
			File.Delete(_libraryPath);
			_libraryPath += ".dll";

			// extract the DLL from our resources
			using (var ins = typeof(Authenticator).Assembly.GetManifestResourceStream("WinAuth.Resources." + libraryname))
			{
				using (var outs = new FileStream(_libraryPath, FileMode.Create, FileAccess.Write))
				{
					var buffer = new byte[4096];
					int read;
					while ((read = ins.Read(buffer, 0, buffer.Length)) != 0)
					{
						outs.Write(buffer, 0, read);
					}
				}
			}

			// load the library
			_library = LoadLibrary(_libraryPath);
			if (_library == IntPtr.Zero)
			{
				int error = Marshal.GetLastWin32Error();
				throw new LibraryNotFoundException("Cannot load the YubiKey AddOn: " + error);
			}
		}

		/// <summary>
		/// Dynamically get the function point to a native DLL entry point
		/// </summary>
		/// <typeparam name="TDelegate">delegate we require</typeparam>
		/// <param name="name">name of function</param>
		/// <returns>function delegate</returns>
		private TDelegate GetFunction<TDelegate>(string name) where TDelegate : class
		{
			IntPtr p = GetProcAddress(_library, name);
			if (p == IntPtr.Zero)
			{
				return null;
			}

			Delegate f = Marshal.GetDelegateForFunctionPointer(p, typeof(TDelegate));

			object obj = f;

			return (TDelegate)obj;
		}

		/// <summary>
		/// Get the last error from the native DLL
		/// </summary>
		/// <returns>last error code</returns>
		public int LastError()
		{
			LastErrorDelegate fe = GetFunction<LastErrorDelegate>("LastError");
			return fe();
		}

		/// <summary>
		/// Set the Challenge/Response for the Yubikey
		/// </summary>
		/// <param name="slot">slot number, only 1 or 2</param>
		/// <param name="key">20 byte key</param>
		/// <param name="keysize">size of key - currently must be 20 bytes</param>
		/// <param name="press">if we require a keypress</param>
		/// <param name="accesscode">current access code if required</param>
		public void SetChallengeResponse(int slot, byte[] key, int keysize, bool press, byte[] accesscode = null)
		{
			LoadLibrary();

			SetChallengeResponseDelegate f = GetFunction<SetChallengeResponseDelegate>("SetChallengeResponse");
			int ret = f(slot, key, keysize, press, accesscode);
			if (ret != 0)
			{
				throw new CannotSetChallengeResponseException(string.Format("Cannot set ChallengeResponse. Error {0}:{1}.", ret, LastError()));
			}
		}

		/// <summary>
		/// Perform the Challenge/Response with the current data
		/// </summary>
		/// <param name="slot">slot number, 1 or 2</param>
		/// <param name="challenge">challenge array, up to 64 bytes</param>
		/// <param name="allowBlock">if we are allow to block (i.e. if keypress = true)</param>
		/// <param name="hash">to save returned hash</param>
		/// <returns>hash result of challengeresponse</returns>
		public byte[] ChallengeResponse(int slot, byte[] challenge, bool allowBlock = true, byte[] hash = null)
		{
			LoadLibrary();

			byte[] maxhash = new byte[64];
			ChallengeResponseDelegate f = GetFunction<ChallengeResponseDelegate>("ChallengeResponse");
			int hashret = f(slot, allowBlock, challenge, challenge.Length, maxhash, maxhash.Length);
			if (hashret != 0) 
			{
				int lasterror = LastError();
				throw new ChallengeResponseException(string.Format("Cannot perform ChallengeResponse. Error {0}:{1}.", hashret, lasterror));
			}

			if (hash == null)
			{
				hash = new byte[20];
			}
			Array.Copy(maxhash, 0, hash, 0, hash.Length);

			return hash;
		}
	}

	/// <summary>
	/// General YubiKey exception
	/// </summary>
	public class YubKeyException : ApplicationException
	{
		public YubKeyException(string msg = null, Exception ex = null) : base(msg, ex) { }
	}

	/// <summary>
	/// Exception if the native library cannot be loaded
	/// </summary>
	public class LibraryNotFoundException : YubKeyException
	{
		public LibraryNotFoundException(string msg = null, Exception ex = null) : base(msg, ex) { }
	}

	/// <summary>
	/// Exception if the Challenge/Response cannot be set
	/// </summary>
	public class CannotSetChallengeResponseException : YubKeyException
	{
		public CannotSetChallengeResponseException(string msg = null, Exception ex = null) : base(msg, ex) { }
	}

	/// <summary>
	/// Exception if the ChallengeResponse cannot be executed
	/// </summary>
	public class ChallengeResponseException : YubKeyException
	{
		public ChallengeResponseException(string msg = null, Exception ex = null) : base(msg, ex) { }
	}
}
