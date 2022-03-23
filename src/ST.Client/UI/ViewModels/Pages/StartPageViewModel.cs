using System;
using System.Application.UI.Resx;
using System.Collections.Generic;
using System.Text;
using System.Windows.Input;
using ReactiveUI;

// ReSharper disable once CheckNamespace
namespace System.Application.UI.ViewModels
{
    public partial class StartPageViewModel
    {
        public class FeatureItem
        {
            public FeatureItem()
            {
                InvokeCommand = ReactiveCommand.Create<object>(OnInvokeCommandExecute);
            }

            public string? Header { get; set; }

            public string? Description { get; set; }

            public string? PreviewImageSource { get; set; }

            public string? PageType { get; init; }

            public ICommand InvokeCommand { get; }

            private void OnInvokeCommandExecute(object parameter)
            {
            }
        }

        public class FeatureGroup
        {
            public string? Header { get; set; }

            public List<FeatureItem>? Items { get; init; }
        }

        public StartPageViewModel()
        {
            FeatureGroups = new()
            {
                new()
                {
                    Header = "新功能上线 🆕~",
                    Items = new()
                    {

                    },
                },
                new()
                {
                    Header = "已有功能 💖~",
                },
            };

        }


        public List<FeatureGroup> FeatureGroups { get; }
    }
}
