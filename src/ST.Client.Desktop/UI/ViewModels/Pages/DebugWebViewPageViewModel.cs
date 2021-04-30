using System;
using System.Collections.Generic;
using System.Text;

namespace System.Application.UI.ViewModels
{
    public class DebugWebViewPageViewModel : TabItemViewModel
    {
        public override string Name
        {
            get => "WebView";
            protected set { throw new NotImplementedException(); }
        }
    }
}
