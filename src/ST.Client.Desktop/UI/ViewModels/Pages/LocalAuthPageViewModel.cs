namespace System.Application.UI.ViewModels
{
    partial class LocalAuthPageViewModel : TabItemViewModel
    {
        public override bool IsTaskBarSubMenu => MenuItems.Any_Nullable();
    }
}