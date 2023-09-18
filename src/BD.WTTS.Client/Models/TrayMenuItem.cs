namespace BD.WTTS.Models;

public partial class TrayMenuItem
{
    public string Key { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public ICommand? Command { get; set; }

    public List<TrayMenuItem>? Items { get; set; }
}
