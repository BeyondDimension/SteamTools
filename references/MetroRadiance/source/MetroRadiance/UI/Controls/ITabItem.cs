using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MetroRadiance.UI.Controls
{
	public interface ITabItem
	{
		string Name { get; }
		int? Badge { get; }
		bool IsSelected { get; set; }
	}
}
