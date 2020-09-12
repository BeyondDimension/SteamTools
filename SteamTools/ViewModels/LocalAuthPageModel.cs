using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SteamTools.ViewModels
{
    public class LocalAuthPageModel: TabItemViewModel
    {
        public override string Name
        {
            get { return Properties.Resources.SteamAuth; }
            protected set { throw new NotImplementedException(); }
        }

    }
}
