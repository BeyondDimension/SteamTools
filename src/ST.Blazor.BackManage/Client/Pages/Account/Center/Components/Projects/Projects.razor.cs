using System.Collections.Generic;
using System.Application.Models;
using Microsoft.AspNetCore.Components;
using AntDesign;

namespace System.Application.Pages.Account.Center
{
    public partial class Projects
    {
        private readonly ListGridType _listGridType = new()
        {
            Gutter = 24,
            Column = 4
        };

        [Parameter]
        public IList<ListItemDataType> List { get; set; }
    }
}