using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AntDesign.Pro.Layout;
using System.Application.Models;
using System.Application.Services;
using Microsoft.AspNetCore.Components;
using AntDesign;

namespace System.Application.Components
{
    public partial class RightContent
    {
        private CurrentUser _currentUser = new();
        private NoticeIconData[] _notifications = { };
        private NoticeIconData[] _messages = { };
        private NoticeIconData[] _events = { };
        private int _count = 0;

        private List<AutoCompleteDataItem<string>> DefaultOptions { get; set; } = new List<AutoCompleteDataItem<string>>
        {
            new AutoCompleteDataItem<string>
            {
                Label = "umi ui",
                Value = "umi ui"
            },
            new AutoCompleteDataItem<string>
            {
                Label = "Pro Table",
                Value = "Pro Table"
            },
            new AutoCompleteDataItem<string>
            {
                Label = "Pro Layout",
                Value = "Pro Layout"
            }
        };

        [Inject] protected NavigationManager NavigationManager { get; set; }

        [Inject] protected IUserService UserService { get; set; }
        [Inject] protected IProjectService ProjectService { get; set; }
        [Inject] protected MessageService MessageService { get; set; }

        protected override async Task OnInitializedAsync()
        {
            await base.OnInitializedAsync();
            SetClassMap();
            _currentUser = await UserService.GetCurrentUserAsync();
            var notices = await ProjectService.GetNoticesAsync();
            _notifications = notices.Where(x => x.Type == "notification").Cast<NoticeIconData>().ToArray();
            _messages = notices.Where(x => x.Type == "message").Cast<NoticeIconData>().ToArray();
            _events = notices.Where(x => x.Type == "event").Cast<NoticeIconData>().ToArray();
            _count = notices.Length;
        }

        protected void SetClassMap()
        {
            ClassMapper
                .Clear()
                .Add("right");
        }

        public void HandleSelectUser(MenuItem item)
        {
            switch (item.Key)
            {
                case "center":
                    NavigationManager.NavigateTo("/account/center");
                    break;
                case "setting":
                    NavigationManager.NavigateTo("/account/settings");
                    break;
                case "logout":
                    NavigationManager.NavigateTo("/user/login");
                    break;
            }
        }

        public void HandleSelectLang(MenuItem item)
        {
        }

        public async Task HandleClear(string key)
        {
            switch (key)
            {
                case "notification":
                    _notifications = new NoticeIconData[] { };
                    break;
                case "message":
                    _messages = new NoticeIconData[] { };
                    break;
                case "event":
                    _events = new NoticeIconData[] { };
                    break;
            }
            await MessageService.Success($"清空了{key}");
        }

        public async Task HandleViewMore(string key)
        {
            await MessageService.Info("Click on view more");
        }
    }
}