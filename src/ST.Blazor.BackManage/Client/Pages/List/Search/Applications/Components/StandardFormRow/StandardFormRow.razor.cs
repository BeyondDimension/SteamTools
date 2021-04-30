using Microsoft.AspNetCore.Components;

namespace System.Application.Pages.List
{
    public partial class StandardFormRow
    {
        [Parameter] public string Title { get; set; }

        [Parameter] public bool Last { get; set; }

        [Parameter] public bool Block { get; set; }

        [Parameter] public bool Grid { get; set; }

        [Parameter] public RenderFragment ChildContent { get; set; }

        protected override void OnInitialized()
        {
            base.OnInitialized();
            SetClassMap();
        }

        protected void SetClassMap()
        {
            ClassMapper
                .Clear()
                .Add("standardFormRow")
                .If("standardFormRowBlock", () => Block)
                .If("standardFormRowLast", () => Last)
                .If("standardFormRowGrid", () => Grid);
        }
    }
}