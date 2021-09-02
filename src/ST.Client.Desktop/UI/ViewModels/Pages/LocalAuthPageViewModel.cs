namespace System.Application.UI.ViewModels
{
    partial class LocalAuthPageViewModel : TabItemViewModel
    {
        public override TabItemId Id => TabItemId.LocalAuth;
        public override bool IsTaskBarSubMenu => MenuItems.Any_Nullable();

        public override string Name
        {
            get => DisplayName;
            protected set { throw new NotImplementedException(); }
        }
    }
}