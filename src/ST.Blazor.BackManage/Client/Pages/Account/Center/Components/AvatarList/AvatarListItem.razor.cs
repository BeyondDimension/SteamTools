using Microsoft.AspNetCore.Components;

namespace System.Application.Pages.Account.Center
{
    public partial class AvatarListItem
    {
        [Parameter] public string Size { get; set; }
        [Parameter] public string Tips { get; set; }
        [Parameter] public string Src { get; set; }
        [Parameter] public EventCallback OnClick { get; set; }

        protected override void OnInitialized()
        {
            base.OnInitialized();
            SetClassMap();
        }

        protected void SetClassMap()
        {
            ClassMapper
                .Clear()
                .Add("avatarItem")
                .If("avatarItemLarge", () => Size == "large")
                .If("avatarItemSmall", () => Size == "small")
                .If("avatarItemMini", () => Size == "mini");
        }
    }
}