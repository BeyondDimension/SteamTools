using ReactiveUI;
using System.Application.UI.Resx;
using System.Properties;

namespace System.Application.UI.ViewModels
{
    public class AddAuthWindowViewModel : WindowViewModel
    {
        public AddAuthWindowViewModel() 
        {
            Title = ThisAssembly.AssemblyTrademark+" | "+ AppResources.LocalAuth_AddAuth;
        }


        
    }
}