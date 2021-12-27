using DynamicData;
using DynamicData.Binding;
using ReactiveUI;
using System.Application.Models;
using System.Application.Services;
using System.Application.UI.Resx;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive.Linq;
namespace System.Application.UI.ViewModels
{
    public class NoticeWindowViewModel : WindowViewModel
    {
        public static string DisplayName => AppResources.NotificationChannelType_Name_Announcement;
         
        public NoticeWindowViewModel()
        {
            Title = GetTitleByDisplayName(DisplayName);
            
        }
 
    }
}