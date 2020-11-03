using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SteamTools.ViewModels
{
    public class AchievementUnlockedPageModel : TabItemViewModel
    {

        public override string Name
        {
            get { return Properties.Resources.Sam; }
            protected set { throw new NotImplementedException(); }
        }

    }
}
