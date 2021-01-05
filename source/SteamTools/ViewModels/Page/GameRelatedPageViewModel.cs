using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SteamTools.ViewModels
{
    public class GameRelatedPageViewModel : TabItemViewModel
    {
        public override string Name
        {
            get { return Properties.Resources.GameRelated; }
            protected set { throw new NotImplementedException(); }
        }

    }
}
