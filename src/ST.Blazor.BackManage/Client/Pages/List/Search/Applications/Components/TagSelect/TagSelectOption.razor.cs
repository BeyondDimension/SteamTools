using Microsoft.AspNetCore.Components;

namespace System.Application.Pages.List
{
    public partial class TagSelectOption
    {
        [Parameter] public string Value { get; set; }

        [Parameter] public bool Checked { get; set; }

        [Parameter] public RenderFragment ChildContent { get; set; }

        [CascadingParameter] public TagSelect Parent { get; set; }

        protected override void OnInitialized()
        {
            base.OnInitialized();
            Parent.AddOption(this);
        }

        protected void HandleCheckedChange(bool isChecked)
        {
            Checked = isChecked;
            if (isChecked)
                Parent.SelectItem(Value);
            else
                Parent.UnSelectItem(Value);
        }

        public void Check(bool isChecked)
        {
            Checked = isChecked;
        }
    }
}