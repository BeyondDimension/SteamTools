namespace Microsoft.Maui;

public static class MauiExtensions
{
    /// <summary>
    /// https://github.com/dotnet/maui/blob/c963d0a5d971a5f3f012480a190a984e3c733d34/src/Controls/src/Core/HandlerImpl/Window/Window.Impl.cs#L164
    /// </summary>
    /// <param name="window"></param>
    /// <returns></returns>
    public static bool IsActivated(this Window window) => window.IsActivated;
}
