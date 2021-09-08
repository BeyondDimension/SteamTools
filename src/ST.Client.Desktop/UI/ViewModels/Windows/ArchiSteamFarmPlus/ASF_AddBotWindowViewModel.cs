using ArchiSteamFarm.Steam;
using Newtonsoft.Json.Linq;
using ReactiveUI;
using System.Application.Models;
using System.Application.UI.Resx;
using System.Properties;

namespace System.Application.UI.ViewModels
{
    public class ASF_AddBotWindowViewModel : WindowViewModel
    {
        public ASF_AddBotWindowViewModel() : base()
        {
            Title = ThisAssembly.AssemblyTrademark + " | 新增Bot";
        }

        public ASF_AddBotWindowViewModel(Bot bot) : this()
        {
            Title = ThisAssembly.AssemblyTrademark + " | 编辑Bot";
            _Bot = bot;
        }

        private Bot? _Bot;
        public Bot? Bot
        {
            get => _Bot;
            set => this.RaiseAndSetIfChanged(ref _Bot, value);
        }


    }
}