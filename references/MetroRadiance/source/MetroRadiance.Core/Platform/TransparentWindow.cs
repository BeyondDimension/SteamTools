using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Interop;
using MetroRadiance.Interop.Win32;

namespace MetroRadiance.Platform
{
	public class TransparentWindow : RawWindow
	{
		public override void Show()
		{
			var parameters = new HwndSourceParameters(this.Name)
			{
				Width = 1,
				Height = 1,
				WindowStyle = (int)WindowStyles.WS_BORDER,
			};

			this.Show(parameters);
		}
	}
}
