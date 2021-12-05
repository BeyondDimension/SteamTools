using ArchiSteamFarm.Steam;
using Newtonsoft.Json.Linq;
using ReactiveUI;
using System.Application.Models;
using System.Application.UI.Resx;
using System.Properties;

// ReSharper disable once CheckNamespace
namespace System.Application.UI.ViewModels
{
    public class ASF_AddBotWindowViewModel : WindowViewModel
    {
        public static string DisplayName_AddBot => "新增Bot";
        public static string DisplayName_EditBott => "编辑Bot";

        protected ASF_AddBotWindowViewModel(string title)
        {
            Title = GetTitleByDisplayName(title);
        }

        public ASF_AddBotWindowViewModel() : this(DisplayName_AddBot)
        {

        }

        public ASF_AddBotWindowViewModel(Bot bot) : this(DisplayName_EditBott)
        {
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
