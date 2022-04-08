using DynamicData;
using DynamicData.Binding;
using Newtonsoft.Json.Linq;
using ReactiveUI;
using System.Application.Models;
using System.Application.Services;
using System.Application.Settings;
using System.Application.UI.Resx;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Properties;
using System.Reactive.Linq;

namespace System.Application.UI.ViewModels
{
    public class SaveEditedAppInfoWindowViewModel : WindowViewModel
    {
        public static string DisplayName => AppResources.GameList_HideGameManger;

        public SaveEditedAppInfoWindowViewModel()
        {
            Title = GetTitleByDisplayName(DisplayName);
        }

    }
}