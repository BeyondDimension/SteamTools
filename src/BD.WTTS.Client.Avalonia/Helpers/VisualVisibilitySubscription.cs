namespace BD.WTTS.UI;

/// <summary>Observe effective visibility while a visual is attached to its current tree.</summary>
public sealed class VisualVisibilitySubscription : IDisposable
{
    readonly Visual[] visuals;
    readonly Action changed;

    public VisualVisibilitySubscription(Visual visual, Action changed)
    {
        this.changed = changed;
        visuals = visual.GetSelfAndVisualAncestors().ToArray();
        foreach (var item in visuals)
            item.PropertyChanged += OnVisualPropertyChanged;
    }

    void OnVisualPropertyChanged(object? sender, AvaloniaPropertyChangedEventArgs e)
    {
        if (e.Property == Visual.IsVisibleProperty)
            changed();
    }

    public void Dispose()
    {
        foreach (var item in visuals)
            item.PropertyChanged -= OnVisualPropertyChanged;
    }
}
