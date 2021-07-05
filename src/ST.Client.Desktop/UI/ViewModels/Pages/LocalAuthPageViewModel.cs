using ReactiveUI;
using System.Reactive;

namespace System.Application.UI.ViewModels
{
    partial class LocalAuthPageViewModel : TabItemViewModel
    {
        public override string Name
        {
            get => DisplayName;
            protected set { throw new NotImplementedException(); }
        }

        public ReactiveCommand<Unit, Unit> AddAuthCommand { get; }

        public ReactiveCommand<Unit, Unit> EncryptionAuthCommand { get; }

        public ReactiveCommand<Unit, Unit> ExportAuthCommand { get; }
    }
}