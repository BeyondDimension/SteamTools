using System.ComponentModel;
using BD.Avalonia8.Image2;
using BD.SteamClient.Models;
using BD.SteamClient.Services;

namespace BD.WTTS.UI.Views.Controls;

/// <summary>Loads Steam covers only while their image control is attached.</summary>
public static class SteamAppImageSource
{
    public static readonly AttachedProperty<SteamApp?> AppProperty =
        AvaloniaProperty.RegisterAttached<Image2, SteamApp?>("App", typeof(SteamAppImageSource));

    public static readonly AttachedProperty<bool> IsHeaderProperty =
        AvaloniaProperty.RegisterAttached<Image2, bool>("IsHeader", typeof(SteamAppImageSource));

    static readonly AttachedProperty<ImageState?> StateProperty =
        AvaloniaProperty.RegisterAttached<Image2, ImageState?>("State", typeof(SteamAppImageSource));

    static SteamAppImageSource()
    {
        AppProperty.Changed.AddClassHandler<Image2>((image, _) => GetState(image).Refresh());
        IsHeaderProperty.Changed.AddClassHandler<Image2>((image, _) => GetState(image).Refresh());
    }

    public static SteamApp? GetApp(Image2 image) => image.GetValue(AppProperty);
    public static void SetApp(Image2 image, SteamApp? app) => image.SetValue(AppProperty, app);
    public static bool GetIsHeader(Image2 image) => image.GetValue(IsHeaderProperty);
    public static void SetIsHeader(Image2 image, bool isHeader) => image.SetValue(IsHeaderProperty, isHeader);

    static ImageState GetState(Image2 image)
    {
        var state = image.GetValue(StateProperty);
        if (state == null)
        {
            state = new ImageState(image);
            image.SetValue(StateProperty, state);
        }
        return state;
    }

    sealed class ImageState
    {
        readonly Image2 image;
        SteamApp? subscribedApp;
        CancellationTokenSource? request;
        bool attached;

        public ImageState(Image2 image)
        {
            this.image = image;
            attached = TopLevel.GetTopLevel(image) != null;
            image.AttachedToVisualTree += OnAttached;
            image.DetachedFromVisualTree += OnDetached;
        }

        void OnAttached(object? sender, VisualTreeAttachmentEventArgs e)
        {
            attached = true;
            Refresh();
        }

        void OnDetached(object? sender, VisualTreeAttachmentEventArgs e)
        {
            attached = false;
            Refresh();
        }

        void OnAppPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (!Dispatcher.UIThread.CheckAccess())
            {
                Dispatcher.UIThread.Post(() =>
                {
                    if (ReferenceEquals(subscribedApp, sender))
                        OnAppPropertyChanged(sender, e);
                });
                return;
            }

            var property = GetIsHeader(image)
                ? nameof(SteamApp.HeaderLogoStream)
                : nameof(SteamApp.LibraryGridStream);
            if (ReferenceEquals(subscribedApp, sender) &&
                (string.IsNullOrEmpty(e.PropertyName) || e.PropertyName == property))
                Refresh();
        }

        public void Refresh()
        {
            request?.Cancel();
            request = null;
            if (subscribedApp != null)
                subscribedApp.PropertyChanged -= OnAppPropertyChanged;
            subscribedApp = null;
            image.Source = null!;

            if (!attached || GetApp(image) is not { } app)
                return;

            subscribedApp = app;
            app.PropertyChanged += OnAppPropertyChanged;
            request = new CancellationTokenSource();
            var kind = GetIsHeader(image) ? SteamApp.LibCacheType.Header : SteamApp.LibCacheType.Library_Grid;
            _ = LoadAsync(app, kind, request);
        }

        async Task LoadAsync(SteamApp app, SteamApp.LibCacheType kind, CancellationTokenSource cancellation)
        {
            Stream? stream = null;
            try
            {
                stream = await Task.Run(() => ISteamService.Instance.GetAppImageAsync(app, kind, cancellation.Token), cancellation.Token).ConfigureAwait(false);
            }
            catch (OperationCanceledException) when (cancellation.IsCancellationRequested)
            {
            }
            catch (Exception ex)
            {
                Log.Error(nameof(SteamAppImageSource), ex, "Steam cover loading failed.");
            }

            try
            {
                await Dispatcher.UIThread.InvokeAsync(() =>
                {
                    if (ReferenceEquals(request, cancellation))
                    {
                        request = null;
                        if (attached && !cancellation.IsCancellationRequested && stream != null)
                        {
                            image.Source = stream;
                            stream = null;
                        }
                    }
                });
            }
            finally
            {
                stream?.Dispose();
                cancellation.Dispose();
            }
        }
    }
}
