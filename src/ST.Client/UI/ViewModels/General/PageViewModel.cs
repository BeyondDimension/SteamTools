using ReactiveUI;
using System;
using System.Application.Services;
using System.Collections.Generic;
using System.Properties;
using System.Runtime.Serialization;
using System.Text;

// ReSharper disable once CheckNamespace
namespace System.Application.UI.ViewModels
{
    public class PageViewModel : ViewModelBase, IPageViewModel
    {
        protected string title = string.Empty;
        [IgnoreDataMember]
        public virtual string Title
        {
            get => title;
            set => this.RaiseAndSetIfChanged(ref title, value);
        }

        protected static string GetTitleByDisplayName(string displayName)
        {
            if (IApplication.IsAvaloniaApp)
            {
                return Constants.HARDCODED_APP_NAME + " | " + displayName;
            }
            else
            {
                return displayName;
            }
        }

        [IgnoreDataMember]
        public bool IsInitialized { get; protected set; }

        public virtual void Initialize()
        {
        }
    }
}
