using Newtonsoft.Json.Linq;
using System.Application.Models;
using System.Application.UI.Resx;
using System.Properties;

namespace System.Application.UI.ViewModels
{
    public class ScriptStoreWindowViewModel : WindowViewModel
    {
        public ScriptStoreWindowViewModel()
        {
            Title = ThisAssembly.AssemblyTrademark + " | " + AppResources.ScriptStore;
        }




    }
}