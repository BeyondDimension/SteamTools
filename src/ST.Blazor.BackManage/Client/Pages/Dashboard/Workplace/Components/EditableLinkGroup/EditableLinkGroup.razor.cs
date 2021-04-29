using Microsoft.AspNetCore.Components;

namespace System.Application.Pages.Dashboard.Workplace
{
    public class EditableLink
    {
        public string Title { get; set; }
        public string Href { get; set; }
        public string Id { get; set; }
    }

    public partial class EditableLinkGroup
    {
        [Parameter] public EditableLink[] Links { get; set; }
    }
}