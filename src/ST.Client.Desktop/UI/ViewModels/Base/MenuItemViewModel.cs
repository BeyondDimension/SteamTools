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

        public virtual string? Header => string.IsNullOrEmpty(name) ? "-" : AppResources.ResourceManager.GetString(name, AppResources.Culture);

        public virtual string? ToolTip => string.IsNullOrEmpty(name) ? null : AppResources.ResourceManager.GetString(name + "Tip", AppResources.Culture);

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

    public class MenuItemCustomName : MenuItemViewModel
    {
        private readonly string? toolTipName;
        public MenuItemCustomName(string? name = null, string? tipName = null)
        {
            Header = name;
            toolTipName = tipName;
        }

        public override string? Header { get; }

        public override string? ToolTip => string.IsNullOrEmpty(toolTipName) ? null : AppResources.ResourceManager.GetString(toolTipName, AppResources.Culture);
    }
    public class MenuItemSeparator : MenuItemViewModel
    {
        public override string? IconKey => null;
        public override string? Header => "-";
        public override string? ToolTip => null;
    }
    public class MenuItemToggle : MenuItemViewModel { }
    public class MenuItemRadio : MenuItemViewModel { }
    public class MenuItemSubItem : MenuItemViewModel { }
}
