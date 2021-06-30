using ReactiveUI;
using System.Collections.Generic;
using System.Windows.Input;
using System.Application.UI.Resx;

namespace System.Application.UI.ViewModels
{
    public class MenuItemViewModel : ReactiveObject
    {
        private readonly string? name;
        public MenuItemViewModel(string? ResourceName = null)
        {
            name = ResourceName;
        }

        private string? _IconKey;
        public virtual string? IconKey
        {
            get => _IconKey;
            set => this.RaiseAndSetIfChanged(ref _IconKey, value);
        }

        public string? Header => string.IsNullOrEmpty(name) ? "-" : AppResources.ResourceManager.GetString(name, AppResources.Culture);

        public string? ToolTip => string.IsNullOrEmpty(name) ? null : AppResources.ResourceManager.GetString(name + "Tip", AppResources.Culture);

        public ICommand? Command { get; set; }
        public object? CommandParameter { get; set; }

        private IList<MenuItemViewModel>? _Items;
        public IList<MenuItemViewModel>? Items
        {
            get => _Items;
            set => this.RaiseAndSetIfChanged(ref _Items, value);
        }

        public void CheckmarkChange(bool isCheck)
        {
            if (isCheck)
                IconKey = "CheckmarkDrawing";
            else
                IconKey = null;
        }
    }
}
