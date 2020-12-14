using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SteamTool.Model;
using SteamTools.Models.Settings;
using SteamTools.Services;

namespace SteamTools.ViewModels
{
    public class WelcomePageViewModel: TabItemViewModel
	{

		public override string Name
		{
			get { return Properties.Resources.Welcome; }
			protected set { throw new NotImplementedException(); }
		}
		public override bool IsShowTab
		{
			get { return GeneralSettings.IsShowStartPage.Value; }
		}

		public MainWindowViewModel Content { get; }

		public WelcomePageViewModel(MainWindowViewModel owner)
		{
			this.Content = owner;
		}

        internal override void Initialize()
		{

		}
    }
}
