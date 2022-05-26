namespace System.Application.UI.ViewModels;

public interface IReadOnlyItemViewType
{
    int ItemViewType { get; }
}

public interface IReadOnlyItemViewType<TItemViewType> : IReadOnlyItemViewType where TItemViewType : Enum
{
    new TItemViewType ItemViewType { get; }

    int IReadOnlyItemViewType.ItemViewType => ItemViewType.ConvertToInt32();
}