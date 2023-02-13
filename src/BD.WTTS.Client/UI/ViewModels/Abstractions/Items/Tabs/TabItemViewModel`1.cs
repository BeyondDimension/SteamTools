// ReSharper disable once CheckNamespace
namespace BD.WTTS.UI.ViewModels.Abstractions;

public abstract partial class TabItemViewModel<TabItemId> : TabItemViewModelBase where TabItemId : struct, Enum
{
    public virtual TabItemId Id { get; }
}