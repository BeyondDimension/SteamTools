using System.Collections.Generic;
using Microsoft.AspNetCore.Components;

namespace System.Application.Pages.List {
  public partial class TagSelect {
    private readonly IList<TagSelectOption> _options = new List<TagSelectOption>();
    private bool _checkedAll;
    private bool _expand = false;

    [Parameter] public bool Expandable { get; set; }

    [Parameter] public bool HideCheckAll { get; set; }

    [Parameter] public string SelectAllText { get; set; } = "All";

    [Parameter] public string CollapseText { get; set; } = "Collapse";

    [Parameter] public string ExpandText { get; set; } = "Expand";

    [Parameter] public IList<string> Value { get; set; }

    [Parameter] public RenderFragment ChildContent { get; set; }

    protected override void OnInitialized() {
      base.OnInitialized();
      SetClassMap();
    }

    protected void SetClassMap() {
      ClassMapper
          .Clear()
          .Add("tagSelect")
          .If("hasExpandTag", () => Expandable)
          .If("expanded", () => _expand);
    }

    private void HandleExpand() {
      _expand = !_expand;
    }

    private void HandleCheckedChange(bool isChecked) {
      _checkedAll = isChecked;
      foreach (var option in _options) option.Check(_checkedAll);
    }

    public void AddOption(TagSelectOption option) {
      _options.Add(option);
    }

    public void SelectItem(string value) {
      Value?.Add(value);
    }

    public void UnSelectItem(string value) {
      Value?.Remove(value);
    }
  }
}