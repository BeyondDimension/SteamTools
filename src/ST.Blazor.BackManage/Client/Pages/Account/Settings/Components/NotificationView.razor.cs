using System.Application.Models;

namespace System.Application.Pages.Account.Settings
{
    public partial class NotificationView
    {
        private readonly UserLiteItem[] _data =
        {
            new UserLiteItem
            {
                Title = "Account Password",
                Description = "Messages from other users will be notified in the form of a station letter"
            },
            new UserLiteItem
            {
                Title = "System Messages",
                Description = "System messages will be notified in the form of a station letter"
            },
            new UserLiteItem
            {
                Title = "To-do Notification",
                Description = "The to-do list will be notified in the form of a letter from the station"
            }
        };
    }
}