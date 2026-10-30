using Avalonia.Controls.Templates;
using FluentAvalonia.UI.Controls;

namespace BD.WTTS.UI.Views.Controls;

/// <summary>Recycles item controls without retaining their template in a global pool.</summary>
public sealed class ScopedRecycleTemplate : IFAElementFactory
{
    readonly FARecyclePool pool = new();

    [Content]
    public IDataTemplate? Template { get; set; }

    public Control GetElement(FAElementFactoryGetArgs args)
        => pool.TryGetElement(string.Empty, args.Parent) ??
           Template?.Build(args.Data) ?? new Border();

    public void RecycleElement(FAElementFactoryRecycleArgs args)
        => pool.PutElement(args.Element, string.Empty, args.Parent);

    public bool Match(object? data) => Template?.Match(data) == true;

    public Control? Build(object? data) => Template?.Build(data);
}
