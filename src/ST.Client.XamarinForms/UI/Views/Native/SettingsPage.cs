using System;
using System.Collections.Generic;
using System.Text;
using VM = System.Application.UI.ViewModels.SettingsPageViewModel;

namespace System.Application.UI.Views.Native
{
    public class SettingsPage : BaseContentPage<VM>
    {
        public SettingsPage()
        {
            ViewModel = VM.Instance;
        }
    }
}
