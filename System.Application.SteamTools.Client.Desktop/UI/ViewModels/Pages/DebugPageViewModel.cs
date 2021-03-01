using ReactiveUI;
using System.Application.UI.Resx;
using System.Collections.Generic;
using System.Linq;

namespace System.Application.UI.ViewModels
{
    public class DebugPageViewModel : TabItemViewModel
    {
        public override string Name
        {
            get => "Debug";
            protected set { throw new NotImplementedException(); }
        }

        private string _DebugString = string.Empty;
        public string DebugString
        {
            get => _DebugString;
            protected set => this.RaiseAndSetIfChanged(ref _DebugString, value);
        }

        public DebugPageViewModel()
        {

        }

        public void DebugButton_Click()
        {
            DebugString += "Test Command" + Environment.NewLine;
        }
    }
}