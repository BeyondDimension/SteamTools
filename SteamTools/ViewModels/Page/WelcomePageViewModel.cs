using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SteamTools.ViewModels
{
    public class WelcomePageViewModel: TabItemViewModel
	{
		public override string Name
		{
			get { return Properties.Resources.Welcome; }
			protected set { throw new NotImplementedException(); }
		}

		public MainWindowViewModel Content { get; }

		public WelcomePageViewModel(MainWindowViewModel owner)
		{
			this.Content = owner;
		}
	}
}
