using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SteamTools.ViewModels
{
    public class AboutPageModel : TabItemViewModel
    {
        public static AboutPageModel Instance { get; } = new AboutPageModel();

        public override string Name
        {
            get { return Properties.Resources.About; }
            protected set { throw new NotImplementedException(); }
        }

    }
}
