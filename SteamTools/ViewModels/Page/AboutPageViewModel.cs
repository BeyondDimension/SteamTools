using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SteamTools.ViewModels
{
    public class AboutPageViewModel : TabItemViewModel
    {
        public static AboutPageViewModel Instance { get; } = new AboutPageViewModel();

        public override string Name
        {
            get { return Properties.Resources.About; }
            protected set { throw new NotImplementedException(); }
        }

    }
}
