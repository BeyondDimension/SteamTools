using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using MetroRadiance.Interop.Win32;

namespace MetroRadiance.UI.Controls
{
	public interface IWindowSettings
	{
		WINDOWPLACEMENT? Placement { get; set; }
		void Reload();
		void Save();
	}


	public class WindowSettings : ApplicationSettingsBase, IWindowSettings
	{
		public WindowSettings(Window window) : base(window.GetType().FullName) { }

		[UserScopedSetting]
		public WINDOWPLACEMENT? Placement
		{
			get { return (WINDOWPLACEMENT?)this["Placement"]; }
			set { this["Placement"] = value; }
		}
	}
}
