using System;
using System.Application.UI.Resx;
using System.Collections.Generic;
using System.Text;

// ReSharper disable once CheckNamespace
namespace System.Application.UI.ViewModels
{
    public partial class StartPageViewModel
    {
        public class FeatureItem
        {

        }

        public class FeatureGroup
        {
            public string Header { get; set; }

            public List<FeatureItem> Controls { get; init; }
        }

        public StartPageViewModel()
        {


        }
    }
}
