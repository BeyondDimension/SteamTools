using System.Collections.Generic;
using AntDesign;

namespace System.Application.Pages.Account.Settings
{
    public partial class Index
    {
        private readonly Dictionary<string, string> _menuMap = new()
        {
            {"base", "Basic Settings"},
            {"security", "Security Settings"},
            {"binding", "Account Binding"},
            {"notification", "New Message Notification"},
        };

        private string _selectKey = "base";

        private void SelectKey(MenuItem item)
        {
            _selectKey = item.Key;
        }
    }
}