// https://docs.microsoft.com/zh-cn/dotnet/maui/fundamentals/windows

using System.Application.UI;
using System.Application.UI.ViewModels;

namespace System.Application.Services.Implementation
{
    /// <inheritdoc cref="IWindowManager"/>
    internal sealed class MauiWindowManagerImpl : IWindowManagerImpl
    {
        readonly IMauiApplication app;

        public MauiWindowManagerImpl(IMauiApplication app)
        {
            this.app = app;
        }

        Type? IWindowManagerImpl.WindowType => typeof(Page);

        public Task Show<TWindowViewModel>(CustomWindow customWindow, TWindowViewModel? viewModel = null, string title = "", ResizeMode resizeMode = ResizeMode.NoResize, bool isDialog = false, bool isParent = true) where TWindowViewModel : WindowViewModel, new()
        {
            throw new NotImplementedException();
        }

        public Task Show(Type typeWindowViewModel, CustomWindow customWindow, WindowViewModel? viewModel = null, string title = "", ResizeMode resizeMode = ResizeMode.NoResize, bool isDialog = false, bool isParent = true)
        {
            throw new NotImplementedException();
        }

        public Task Show(CustomWindow customWindow, WindowViewModel? viewModel = null, string title = "", ResizeMode resizeMode = ResizeMode.NoResize, bool isDialog = false, bool isParent = true)
        {
            throw new NotImplementedException();
        }

        public Task<bool> ShowDialog<TWindowViewModel>(CustomWindow customWindow, TWindowViewModel? viewModel = null, string title = "", ResizeMode resizeMode = ResizeMode.NoResize, bool isDialog = true, bool isParent = true) where TWindowViewModel : WindowViewModel, new()
        {
            throw new NotImplementedException();
        }

        public Task ShowDialog(Type typeWindowViewModel, CustomWindow customWindow, WindowViewModel? viewModel = null, string title = "", ResizeMode resizeMode = ResizeMode.NoResize, bool isDialog = true)
        {
            throw new NotImplementedException();
        }

        public Task ShowDialog(CustomWindow customWindow, WindowViewModel? viewModel = null, string title = "", ResizeMode resizeMode = ResizeMode.NoResize, bool isDialog = true)
        {
            throw new NotImplementedException();
        }
    }
}
