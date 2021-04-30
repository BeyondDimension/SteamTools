using System.Collections.Generic;
using System.Threading.Tasks;
using AntDesign;
using System.Application.Models;
using System.Application.Services;
using Microsoft.AspNetCore.Components;
using System.Application.Pages.Form;

namespace System.Application.Pages.List
{
    public partial class Projects
    {
        private readonly ListGridType _listGridType = new()
        {
            Gutter = 16,
            Xs = 1,
            Sm = 2,
            Md = 3,
            Lg = 3,
            Xl = 4,
            Xxl = 4,
        };

        private readonly FormItemLayout _formItemLayout = new()
        {
            WrapperCol = new ColLayoutParam
            {
                Xs = new EmbeddedProperty { Span = 24},
                Sm = new EmbeddedProperty { Span = 16},
            }
        };

        private readonly ListFormModel _model = new();

        private IList<ListItemDataType> _fakeList = new List<ListItemDataType>();

        [Inject] public IProjectService ProjectService { get; set; }

        protected override async Task OnInitializedAsync()
        {
            await base.OnInitializedAsync();
            _fakeList = await ProjectService.GetFakeListAsync(8);
        }
    }
}