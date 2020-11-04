using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SteamTools.ViewModels
{
	public class SettingsPageViewModel : TabItemViewModel
	{
		public static SettingsPageViewModel Instance { get; } = new SettingsPageViewModel();

		public override string Name
		{
			get { return Properties.Resources.Settings; }
			protected set { throw new NotImplementedException(); }
		}

	}
}
