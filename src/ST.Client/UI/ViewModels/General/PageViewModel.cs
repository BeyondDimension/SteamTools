using ReactiveUI;
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

        public static string GetTitleByDisplayName(string displayName)
        {
            if (OperatingSystem2.Application.UseAvalonia)
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
