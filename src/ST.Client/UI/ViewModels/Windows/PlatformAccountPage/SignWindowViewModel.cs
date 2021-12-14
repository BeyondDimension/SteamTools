using DynamicData;
using DynamicData.Binding;
using ReactiveUI;
using System.Application.Models;
using System.Application.Services;
using System.Application.UI.Resx;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive.Linq;
namespace System.Application.UI.ViewModels
{
    public class SignWindowViewModel : WindowViewModel
    {
        public static string DisplayName => AppResources.ScriptStore;

        public SignWindowViewModel()
        {
            Title = GetTitleByDisplayName(DisplayName);
            
        }
 
    }
}