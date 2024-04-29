namespace BD.WTTS.Models;

public partial class TrayMenuItem
{
    public string Key { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public ICommand? Command { get; set; }

    public object? CommandParameter { get; set; }

    public List<TrayMenuItem>? Items { get; set; }

    public object? IsVisible { get; set; }

    public object? IsEnabled { get; set; }

    public int Order { get; set; }
}
