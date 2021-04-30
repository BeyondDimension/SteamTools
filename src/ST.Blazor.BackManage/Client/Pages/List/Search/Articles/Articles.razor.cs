using System.Collections.Generic;
using System.Threading.Tasks;
using System.Application.Models;
using System.Application.Services;
using Microsoft.AspNetCore.Components;

namespace System.Application.Pages.List {
  public partial class Articles {
    private readonly string[] _defaultOwners = {"wzj", "wjh"};
    private readonly ListFormModel _model = new();

    private readonly Owner[] _owners =
        {
            new Owner {Id = "wzj", Name = "Myself"},
            new Owner {Id = "wjh", Name = "Wu Jiahao"},
            new Owner {Id = "zxx", Name = "Zhou Xingxing"},
            new Owner {Id = "zly", Name = "Zhao Liying"},
            new Owner {Id = "ym", Name = "Yao Ming"}
        };

    private IList<ListItemDataType> _fakeList = new List<ListItemDataType>();

    [Inject] public IProjectService ProjectService { get; set; }

    private void SetOwner() {
    }

    protected override async Task OnInitializedAsync() {
      await base.OnInitializedAsync();
      _fakeList = await ProjectService.GetFakeListAsync(8);
    }
  }
}