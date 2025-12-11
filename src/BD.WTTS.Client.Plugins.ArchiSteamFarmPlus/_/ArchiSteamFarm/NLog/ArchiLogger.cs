// ReSharper disable once CheckNamespace
namespace ArchiSteamFarm.NLog;

public sealed class ArchiLogger
{
    readonly ILogger? Logger;

    public ArchiLogger(string name)
    {
        Logger = Ioc.Get_Nullable<ILoggerFactory>()?.CreateLogger(name);
    }

    public void LogGenericDebug(string message, [CallerMemberName] string? previousMethodName = null)
    {
        Logger?.LogDebug($"{previousMethodName}() {message}");
    }

    public void LogGenericDebuggingException(Exception exception, [CallerMemberName] string? previousMethodName = null)
    {
        Logger?.LogDebug(exception, $"{previousMethodName}()");
    }

    public void LogGenericError(string message, [CallerMemberName] string? previousMethodName = null)
    {
        Logger?.LogError($"{previousMethodName}() {message}");
    }

    public void LogGenericException(Exception exception, [CallerMemberName] string? previousMethodName = null)
    {
        Logger?.LogError(exception, $"{previousMethodName}()");
    }

    public void LogGenericInfo(string message, [CallerMemberName] string? previousMethodName = null)
    {
        Logger?.LogInformation($"{previousMethodName}() {message}");
    }

    public void LogGenericTrace(string message, [CallerMemberName] string? previousMethodName = null)
    {
        Logger?.LogTrace($"{previousMethodName}() {message}");
    }

    public void LogGenericWarning(string message, [CallerMemberName] string? previousMethodName = null)
    {
        Logger?.LogWarning($"{previousMethodName}() {message}");
    }

    public void LogGenericWarningException(Exception exception, [CallerMemberName] string? previousMethodName = null)
    {
        Logger?.LogWarning(exception, $"{previousMethodName}()");
    }

    public void LogNullError(object? nullObject, [CallerArgumentExpression("nullObject")] string? nullObjectName = null, [CallerMemberName] string? previousMethodName = null)
    {
        LogGenericError(string.Format(CultureInfo.CurrentCulture, Strings.ErrorObjectIsNull, nullObjectName), previousMethodName);
    }
}
