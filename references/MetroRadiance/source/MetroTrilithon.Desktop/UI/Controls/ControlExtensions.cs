using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace MetroTrilithon.UI.Controls
{
	public static class Extensions
	{
		public static Dock Reverse(this Dock d)
		{
			switch (d)
			{
				case Dock.Top:
					return Dock.Bottom;
				case Dock.Left:
					return Dock.Right;
				case Dock.Right:
					return Dock.Left;
				case Dock.Bottom:
					return Dock.Top;
			}

			return d;
		}
	}
}
