using System;
using System.Collections.Generic;
using System.Linq;
using System.Management;

namespace MetroTrilithon.Desktop
{
	public class SystemEnvironment
	{
		public string OS { get; }
		public string OSVersion { get; }
		public string Architecture { get; }

		public string CPU { get; }
		public string TotalPhysicalMemorySize { get; }
		public string FreePhysicalMemorySize { get; }

		public string DotNetVersion { get; }

		public string ErrorMessage { get; }

		public SystemEnvironment()
		{
			try
			{
				using (var managementClass = new ManagementClass("Win32_OperatingSystem"))
				using (var managementObject = managementClass.GetInstances().OfType<ManagementObject>().FirstOrDefault())
				{
					if (managementObject == null) return;

					this.OS = managementObject["Caption"].ToString();
					this.OSVersion = managementObject["Version"].ToString();
					this.Architecture = managementObject["OSArchitecture"].ToString();

					this.TotalPhysicalMemorySize = $"{managementObject["TotalVisibleMemorySize"]:N0} KB";
					this.FreePhysicalMemorySize = $"{managementObject["FreePhysicalMemory"]:N0} KB";
				}


				using (var managementClass = new ManagementClass("Win32_Processor"))
				using (var managementObject = managementClass.GetInstances().OfType<ManagementObject>().FirstOrDefault())
				{
					if (managementObject == null) return;

					this.CPU = managementObject["Name"].ToString();
				}

				this.DotNetVersion = Desktop.DotNetVersion.GetVersion();
			}
			catch (Exception ex)
			{
				this.ErrorMessage = ex.Message;
			}
		}

		public override string ToString()
		{
			if (!string.IsNullOrEmpty(this.ErrorMessage))
			{
				return $@"SystemEnvironment
({this.ErrorMessage})";
			}

			return $@"SystemEnvironment
OS:           {this.OS}
OSVersion:    {this.OSVersion}
Architecture: {this.Architecture}
Runtime:      {this.DotNetVersion}

CPU:         {this.CPU}
RAM (Total): {this.TotalPhysicalMemorySize}
RAM (Free):  {this.FreePhysicalMemorySize}";

		}
	}
}
