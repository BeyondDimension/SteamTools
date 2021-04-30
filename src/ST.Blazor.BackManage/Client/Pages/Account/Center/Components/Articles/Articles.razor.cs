using System.Collections.Generic;
using System.Application.Models;
using Microsoft.AspNetCore.Components;

namespace System.Application.Pages.Account.Center
{
    public partial class Articles
    {
        [Parameter] public IList<ListItemDataType> List { get; set; }
    }
}