using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace MetroRadiance.UI.Controls
{
	public class TabView : ListBox
	{
		static TabView()
		{
			DefaultStyleKeyProperty.OverrideMetadata(typeof(TabView), new FrameworkPropertyMetadata(typeof(TabView)));
		}

		protected override void OnSelectionChanged(SelectionChangedEventArgs e)
		{
			base.OnSelectionChanged(e);

			foreach (var item in e.RemovedItems.OfType<ITabItem>())
			{
				item.IsSelected = false;
			}
			foreach (var item in e.AddedItems.OfType<ITabItem>())
			{
				item.IsSelected = true;
			}
		}
	}
}
