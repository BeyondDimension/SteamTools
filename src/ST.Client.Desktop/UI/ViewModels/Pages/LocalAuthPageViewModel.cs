namespace System.Application.UI.ViewModels
{
    partial class LocalAuthPageViewModel : TabItemViewModel, MainWindowViewModel.ITabItemViewModel
    {
        MainWindowViewModel.TabItemId MainWindowViewModel.ITabItemViewModel.Id
             => MainWindowViewModel.TabItemId.LocalAuth;

        public override string Name
        {
            get => DisplayName;
            protected set { throw new NotImplementedException(); }
        }
    }
}