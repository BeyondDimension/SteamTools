using System.Threading.Tasks;
using System.Application.Models;
using System.Application.Services;
using Microsoft.AspNetCore.Components;

namespace System.Application.Pages.Profile
{
    public partial class Basic
    {
        private BasicProfileDataType _data = new();

        [Inject] protected IProfileService ProfileService { get; set; }

        protected override async Task OnInitializedAsync()
        {
            await base.OnInitializedAsync();
            _data = await ProfileService.GetBasicAsync();
        }
    }
}