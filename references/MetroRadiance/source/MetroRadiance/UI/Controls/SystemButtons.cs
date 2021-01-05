using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace MetroRadiance.UI.Controls
{
	public class SystemButtons : Control
	{
		static SystemButtons()
		{
			DefaultStyleKeyProperty.OverrideMetadata(typeof(SystemButtons), new FrameworkPropertyMetadata(typeof(SystemButtons)));
			IsTabStopProperty.OverrideMetadata(typeof(SystemButtons), new FrameworkPropertyMetadata(false));
		}
	}
}
