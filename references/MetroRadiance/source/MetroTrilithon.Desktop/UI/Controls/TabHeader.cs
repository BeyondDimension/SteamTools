using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace MetroTrilithon.UI.Controls
{
	public class TabHeader : ListBox
	{
		static TabHeader()
		{
			DefaultStyleKeyProperty.OverrideMetadata(
				typeof(TabHeader),
				new FrameworkPropertyMetadata(typeof(TabHeader)));
		}
	}
}
