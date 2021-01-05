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
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;

using Microsoft.Win32.SafeHandles;

namespace WinAuth
{
	/// <summary>
	/// Class that loads all current USB devices so we can find the YubiKey
	/// Credit to http://brandonw.net/
	/// </summary>
	public class HIDDevice : IDisposable
	{
		#region P/Invoke

		[Flags]
		private enum EFileAttributes : uint
		{
			Readonly = 0x00000001,
			Hidden = 0x00000002,
			System = 0x00000004,
			Directory = 0x00000010,
			Archive = 0x00000020,
			Device = 0x00000040,
			Normal = 0x00000080,
			Temporary = 0x00000100,
			SparseFile = 0x00000200,
			ReparsePoint = 0x00000400,
			Compressed = 0x00000800,
			Offline = 0x00001000,
			NotContentIndexed = 0x00002000,
			Encrypted = 0x00004000,
			Write_Through = 0x80000000,
			Overlapped = 0x40000000,
			NoBuffering = 0x20000000,
			RandomAccess = 0x10000000,
			SequentialScan = 0x08000000,
			DeleteOnClose = 0x04000000,
			BackupSemantics = 0x02000000,
			PosixSemantics = 0x01000000,
			OpenReparsePoint = 0x00200000,
			OpenNoRecall = 0x00100000,
			FirstPipeInstance = 0x00080000
		}

		[StructLayout(LayoutKind.Sequential)]
		private struct HIDD_ATTRIBUTES
		{
			public Int32 Size;
			public Int16 VendorID;
			public Int16 ProductID;
			public Int16 VersionNumber;
		}

		//[StructLayout(LayoutKind.Sequential)]
		//private struct GUID
		//{
		//	public int Data1;
		//	public System.UInt16 Data2;
		//	public System.UInt16 Data3;
		//	[MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
		//	public byte[] data4;
		//}

		[DllImport("hid.dll", SetLastError = true)]
		private static extern void HidD_GetHidGuid(
			ref Guid lpHidGuid);

		[DllImport(@"setupapi.dll", CharSet = CharSet.Auto, SetLastError = true)]
		private static extern IntPtr SetupDiGetClassDevs(
			ref Guid ClassGuid,
			IntPtr Enumerator, //[MarshalAs(UnmanagedType.LPTStr)] string Enumerator,
			IntPtr hwndParent,
			UInt32 Flags
			);

		[StructLayout(LayoutKind.Sequential)]
		private struct SP_DEVICE_INTERFACE_DATA
		{
			public Int32 cbSize;
			public Guid InterfaceClassGuid;
			public Int32 Flags;
			public UIntPtr Reserved;
		}

		[StructLayout(LayoutKind.Sequential, Pack = 1)]
		private struct SP_DEVICE_INTERFACE_DETAIL_DATA
		{
			public UInt32 cbSize;
			[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)]
			public string DevicePath;
		}

		[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
		private struct PSP_DEVICE_INTERFACE_DETAIL_DATA
		{
			public Int32 cbSize;
			[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)]
			public string DevicePath;
		}

		[DllImport(@"setupapi.dll", SetLastError = true)]
		private static extern Boolean SetupDiGetDeviceInterfaceDetail(
			IntPtr hDevInfo,
			ref SP_DEVICE_INTERFACE_DATA deviceInterfaceData,
			IntPtr deviceInterfaceDetailData,
			UInt32 deviceInterfaceDetailDataSize,
			out UInt32 requiredSize,
			IntPtr deviceInfoData
		);

		[DllImport(@"setupapi.dll", SetLastError = true)]
		private static extern Boolean SetupDiGetDeviceInterfaceDetail(
			IntPtr hDevInfo,
			ref SP_DEVICE_INTERFACE_DATA deviceInterfaceData,
			ref SP_DEVICE_INTERFACE_DETAIL_DATA deviceInterfaceDetailData,
			UInt32 deviceInterfaceDetailDataSize,
			out UInt32 requiredSize,
			IntPtr deviceInfoData
		);

		[DllImport(@"setupapi.dll", CharSet = CharSet.Auto, SetLastError = true)]
		private static extern Boolean SetupDiEnumDeviceInterfaces(
			IntPtr hDevInfo,
			//ref SP_DEVINFO_DATA devInfo,
			IntPtr devInvo,
			ref Guid interfaceClassGuid,
			Int32 memberIndex,
			ref SP_DEVICE_INTERFACE_DATA deviceInterfaceData
		);

		[DllImport(@"setupapi.dll", CharSet = CharSet.Auto, SetLastError = true)]
		private static extern UInt16 SetupDiDestroyDeviceInfoList(IntPtr hDevInfo);

		[DllImport("Kernel32.dll", SetLastError = true, CharSet = CharSet.Auto)]
		static extern SafeFileHandle CreateFile(
			string lpFileName,
			UInt32 dwDesiredAccess,
			UInt32 dwShareMode,
			IntPtr lpSecurityAttributes,
			UInt32 dwCreationDisposition,
			UInt32 dwFlagsAndAttributes,
			IntPtr hTemplateFile
		);

		[DllImport("kernel32.dll")]
		private static extern int CloseHandle(SafeFileHandle hObject);
		[DllImport("kernel32.dll")]
		private static extern int CloseHandle(IntPtr hObject);

		[DllImport("hid.dll", SetLastError = true)]
		private static extern int HidD_GetPreparsedData(
			SafeFileHandle hObject,
			ref IntPtr pPHIDP_PREPARSED_DATA);

		[DllImport("hid.dll", SetLastError = true)]
		private static extern int HidP_GetCaps(
			IntPtr pPHIDP_PREPARSED_DATA,
			ref HIDP_CAPS myPHIDP_CAPS);

		[DllImport("hid.dll")]
		internal extern static bool HidD_SetOutputReport(
			IntPtr HidDeviceObject,
			byte[] lpReportBuffer,
			uint ReportBufferLength);

		[DllImport("hid.dll")]
		private static extern Boolean HidD_GetAttributes(IntPtr HidDeviceObject, ref HIDD_ATTRIBUTES Attributes);

		[DllImport("kernel32.dll")]
		private static extern int WriteFile(SafeFileHandle hFile, ref byte lpBuffer, int nNumberOfBytesToWrite, ref int lpNumberOfBytesWritten, int lpOverlapped);

		[StructLayout(LayoutKind.Sequential)]
		private struct HIDP_CAPS
		{
			public UInt16 Usage;
			public UInt16 UsagePage;
			public UInt16 InputReportByteLength;
			public UInt16 OutputReportByteLength;
			public UInt16 FeatureReportByteLength;
			[MarshalAs(UnmanagedType.ByValArray, SizeConst = 17)]
			public UInt16[] Reserved;
			public UInt16 NumberLinkCollectionNodes;
			public UInt16 NumberInputButtonCaps;
			public UInt16 NumberInputValueCaps;
			public UInt16 NumberInputDataIndices;
			public UInt16 NumberOutputButtonCaps;
			public UInt16 NumberOutputValueCaps;
			public UInt16 NumberOutputDataIndices;
			public UInt16 NumberFeatureButtonCaps;
			public UInt16 NumberFeatureValueCaps;
			public UInt16 NumberFeatureDataIndices;
		}

		private const int DIGCF_PRESENT = 0x00000002;
		private const int DIGCF_DEVICEINTERFACE = 0x00000010;
		private const int DIGCF_INTERFACEDEVICE = 0x00000010;
		private const uint GENERIC_READ = 0x80000000;
		private const uint GENERIC_WRITE = 0x40000000;
		private const uint FILE_SHARE_READ = 0x00000001;
		private const uint FILE_SHARE_WRITE = 0x00000002;
		private const int OPEN_EXISTING = 3;

		#endregion

		#region Declarations

		private bool _found = false;
		private Guid _guid;
		private IntPtr _hDeviceInfo = IntPtr.Zero;
		private SP_DEVICE_INTERFACE_DATA _SP_DEVICE_INTERFACE_DATA;
		private string _devicePath;
		private SafeFileHandle _hidHandle;
		private FileStream _stream;

		#endregion

		#region Public Events

		public event EventHandler<HIDDeviceDataReceivedEventArgs> DataReceived;

		#endregion

		#region Constructors / Teardown

		public HIDDevice(string devicePath)
		{
			_Init(devicePath, false);
		}

		public HIDDevice(int vendorId, int productId)
		{
			_Init(vendorId, productId, false);
		}

		public HIDDevice(int vendorId, int productId, bool throwNotFoundError)
		{
			_Init(vendorId, productId, throwNotFoundError);
		}

		public HIDDevice(string devicePath, bool throwNotFoundError)
		{
			_Init(devicePath, throwNotFoundError);
		}

		#endregion

		#region Public Properties

		public int InputReportLength { get; private set; }
		public int OutputReportLength { get; private set; }

		public bool Found
		{
			get
			{
				return _found;
			}
		}

		#endregion

		#region Public Methods

		public class HIDDeviceEntry
		{
			public string Path { get; set; }
			public int VendorID { get; set; }
			public int ProductID { get; set; }

			public HIDDeviceEntry(string path, int vid, int pid)
			{
				Path = path;
				VendorID = vid;
				ProductID = pid;
			}
		}

		public static List<HIDDeviceEntry> GetAllDevices(int? vendorId = null, int? productId = null)
		{
			return GetAllDevices(new Guid(Guid.Empty.ToString()), vendorId, productId);
		}
		public static List<HIDDeviceEntry> GetAllDevices(Guid guid, int? vendorId = null, int? productId = null)
		{
			int index = 0;
			//GUID guid = new GUID();
			var devices = new List<HIDDeviceEntry>();

			if (guid == Guid.Empty)
			{
				HidD_GetHidGuid(ref guid);
			}
			IntPtr devicesHandle = SetupDiGetClassDevs(ref guid, IntPtr.Zero, IntPtr.Zero, DIGCF_INTERFACEDEVICE | DIGCF_PRESENT);
			var diData = new SP_DEVICE_INTERFACE_DATA();
			diData.cbSize = Marshal.SizeOf(diData);

			while (SetupDiEnumDeviceInterfaces(devicesHandle, IntPtr.Zero, ref guid, index, ref diData))
			{
				//Get the buffer size
				UInt32 size;
				SetupDiGetDeviceInterfaceDetail(devicesHandle, ref diData, IntPtr.Zero, 0, out size, IntPtr.Zero);

				// Uh...yeah.
				var diDetail = new SP_DEVICE_INTERFACE_DETAIL_DATA();
				diDetail.cbSize = (uint)(IntPtr.Size == 8 ? 8 : 5);

				//Get detailed information
				if (SetupDiGetDeviceInterfaceDetail(devicesHandle, ref diData, ref diDetail, size, out size, IntPtr.Zero))
				{
					//Get a handle to this device
					var handle = CreateFile(diDetail.DevicePath, 4 /*GENERIC_WRITE*/, 3 /*FILE_SHARE_READ | FILE_SHARE_WRITE*/, IntPtr.Zero, 4 /*OPEN_EXISTING*/, 0, IntPtr.Zero);
					if (handle.IsInvalid == false)
					{
						//Get this device's attributes
						var attrib = new HIDD_ATTRIBUTES();
						attrib.Size = Marshal.SizeOf(attrib);
						if (HidD_GetAttributes(handle.DangerousGetHandle(), ref attrib))
						{
							int vid = attrib.VendorID & 0xFFFF;
							int pid = attrib.ProductID & 0xFFFF;
							//See if this is one we care about
							if ((!vendorId.HasValue || vid == vendorId.Value) &&
								(!productId.HasValue || pid == productId.Value))
							{
								devices.Add(new HIDDeviceEntry(diDetail.DevicePath, vid, pid));
								break;
							}
						}
					}

					//Close the handle
					handle.Close();
					//CloseHandle(handle);
				}

				//Move on
				index++;
			}

			SetupDiDestroyDeviceInfoList(devicesHandle);

			return devices;
		}

		public static bool IsDetected(int vendorId, int productId)
		{
			var device = new HIDDevice(vendorId, productId, false);
			bool ret = device.Found;

			device.Dispose();

			return ret;
		}

		public void Write(byte reportType, byte[] data)
		{
			int bytesSent = 0;

			do
			{
				byte[] buffer = new byte[OutputReportLength];
				buffer[0] = reportType;
				for (int i = 1; i < buffer.Length; i++)
				{
					if (bytesSent < data.Length)
					{
						buffer[i] = data[bytesSent];
						bytesSent++;
					}
					else
					{
						buffer[i] = 0;
					}
				}

				HidD_SetOutputReport(_hidHandle.DangerousGetHandle(), buffer, (uint)buffer.Length);
			} while (bytesSent < data.Length);
		}

		public void Disconnect()
		{
			try
			{
				_stream.Close();
			}
			catch
			{
			}

			try
			{
				CloseHandle(_hidHandle);
			}
			catch
			{
			}

			SetupDiDestroyDeviceInfoList(_hDeviceInfo);
		}

		public void Dispose()
		{
			try
			{
				Disconnect();
			}
			catch
			{
				//Don't care...
			}
		}

		#endregion

		#region Private Methods

		private void _Init(int vendorId, int productId, bool throwNotFoundError)
		{
			var devices = HIDDevice.GetAllDevices(vendorId, productId);

			if (devices != null && devices.Count > 0)
			{
				_Init(devices[0].Path, throwNotFoundError);
			}
			else
			{
				if (throwNotFoundError)
					throw new InvalidOperationException("Device not found");
			}
		}

		private void _Init(string devicePath, bool throwNotFoundError)
		{
			bool result;
			int deviceCount = 0;
			uint size;
			uint requiredSize;

			_guid = new Guid();
			HidD_GetHidGuid(ref _guid);

			_hDeviceInfo = SetupDiGetClassDevs(ref _guid, IntPtr.Zero, IntPtr.Zero, DIGCF_INTERFACEDEVICE | DIGCF_PRESENT);

			do
			{
				_SP_DEVICE_INTERFACE_DATA = new SP_DEVICE_INTERFACE_DATA();
				_SP_DEVICE_INTERFACE_DATA.cbSize = Marshal.SizeOf(_SP_DEVICE_INTERFACE_DATA);
				result = SetupDiEnumDeviceInterfaces(_hDeviceInfo, IntPtr.Zero, ref _guid, deviceCount, ref _SP_DEVICE_INTERFACE_DATA);
				SetupDiGetDeviceInterfaceDetail(_hDeviceInfo, ref _SP_DEVICE_INTERFACE_DATA, IntPtr.Zero, 0, out requiredSize, IntPtr.Zero);
				size = requiredSize;
				var diDetail = new SP_DEVICE_INTERFACE_DETAIL_DATA();
				diDetail.cbSize = (uint)(IntPtr.Size == 8 ? 8 : 5);
				SetupDiGetDeviceInterfaceDetail(_hDeviceInfo, ref _SP_DEVICE_INTERFACE_DATA, ref diDetail,
					size, out requiredSize, IntPtr.Zero);
				_devicePath = diDetail.DevicePath;

				if (_devicePath == devicePath)
				{
					_found = true;
					_SP_DEVICE_INTERFACE_DATA = new SP_DEVICE_INTERFACE_DATA();
					_SP_DEVICE_INTERFACE_DATA.cbSize = Marshal.SizeOf(_SP_DEVICE_INTERFACE_DATA);
					SetupDiEnumDeviceInterfaces(_hDeviceInfo, IntPtr.Zero, ref _guid, deviceCount, ref _SP_DEVICE_INTERFACE_DATA);
					size = 0;
					requiredSize = 0;
					SetupDiGetDeviceInterfaceDetail(_hDeviceInfo, ref _SP_DEVICE_INTERFACE_DATA, IntPtr.Zero, size, out requiredSize, IntPtr.Zero);
					SetupDiGetDeviceInterfaceDetail(_hDeviceInfo, ref _SP_DEVICE_INTERFACE_DATA, IntPtr.Zero, size, out requiredSize, IntPtr.Zero);
					_hidHandle = CreateFile(_devicePath, (uint)FileAccess.ReadWrite, (uint)FileShare.ReadWrite, IntPtr.Zero, (uint)FileMode.Open, (uint)EFileAttributes.Overlapped, IntPtr.Zero);

					//Get report lengths
					IntPtr preparsedDataPtr = (IntPtr)0xffffffff;
					if (HidD_GetPreparsedData(_hidHandle, ref preparsedDataPtr) != 0)
					{
						var caps = new HIDP_CAPS();
						HidP_GetCaps(preparsedDataPtr, ref caps);
						OutputReportLength = caps.OutputReportByteLength;
						InputReportLength = caps.InputReportByteLength;

						_stream = new FileStream(_hidHandle, FileAccess.ReadWrite, InputReportLength, true);
						var buffer = new byte[InputReportLength];
						_stream.BeginRead(buffer, 0, buffer.Length, new AsyncCallback(OnReadData), buffer);
					}

					break;
				}

				deviceCount++;
			} while (result);

			if (!_found)
			{
				if (throwNotFoundError)
					throw new InvalidOperationException("Device not found");
			}
		}

		private void OnReadData(IAsyncResult result)
		{
			var buffer = (byte[])result.AsyncState;

			try
			{
				_stream.EndRead(result);
				var receivedData = new byte[InputReportLength - 1];
				Array.Copy(buffer, 1, receivedData, 0, receivedData.Length);

				if (receivedData != null)
				{
					if (DataReceived != null)
					{
						DataReceived(this, new HIDDeviceDataReceivedEventArgs(buffer[0], receivedData));
					}
				}

				var buf = new byte[buffer.Length];
				_stream.BeginRead(buf, 0, buffer.Length, new AsyncCallback(OnReadData), buf);
			}
			catch
			{
			}
		}

		#endregion
	}

  public class HIDDeviceDataReceivedEventArgs : EventArgs
	{
    #region Declarations

    private byte _reportType;
    private byte[] _data;

    #endregion

    #region Constructors / Teardown

		public HIDDeviceDataReceivedEventArgs(byte reportType, byte[] data)
    {
      _reportType = reportType;
      _data = data;
    }

    #endregion

    #region Public Properties

    public byte ReportType
    {
      get
      {
        return _reportType;
      }
    }

    public byte[] Data
    {
      get
      {
        return _data;
      }
    }

    #endregion
  }
}
