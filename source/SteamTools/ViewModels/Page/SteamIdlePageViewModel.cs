using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SteamTools.ViewModels
{
    public class SteamIdlePageViewModel : TabItemViewModel
    {

        public override string Name
        {
            get { return Properties.Resources.IdleCard; }
            protected set { throw new NotImplementedException(); }
        }

    }
}
