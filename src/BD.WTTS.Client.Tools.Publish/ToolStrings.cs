internal static class ToolStrings
{
    static readonly System.Resources.ResourceManager Resources = new("ToolStrings", typeof(ToolStrings).Assembly);

    internal static string Get(string name) => Resources.GetString(name)!;
}
