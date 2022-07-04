using Android.Content;

// ReSharper disable once CheckNamespace
namespace System.Application.UI;

public interface IHandleUri
{
    void HandleUri(Uri uri);
}

public static partial class HandleUriExtensions
{
    public static void HandleUri(this IHandleUri thiz, Intent? intent)
        => thiz.HandleUri(intent?.DataString);

    public static void HandleUri(this IHandleUri thiz, string? uriString)
    {
        if (!string.IsNullOrWhiteSpace(uriString))
        {
            try
            {
                thiz.HandleUri(new Uri(uriString));
            }
            catch
            {

            }
        }
    }
}
