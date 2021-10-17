using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Application.UI.ViewModels;
using System.Reflection;

namespace System.Application.Services
{
    /// <summary>
    /// 窗口管理
    /// </summary>
    public interface IWindowManager : IService<IWindowManager>
    {
        /// <summary>
        /// 显示一个窗口
        /// </summary>
        /// <typeparam name="TWindowViewModel"></typeparam>
        /// <param name="customWindow"></param>
        /// <param name="viewModel"></param>
        /// <param name="title"></param>
        /// <param name="resizeMode"></param>
        /// <param name="isDialog"></param>
        /// <returns></returns>
        Task Show<TWindowViewModel>(
            CustomWindow customWindow,
            TWindowViewModel? viewModel = null,
            string title = "",
            ResizeMode resizeMode = ResizeMode.NoResize,
            bool isDialog = false,
            bool isParent = true)
            where TWindowViewModel : WindowViewModel, new();

        /// <inheritdoc cref="Show{TWindowViewModel}(CustomWindow, TWindowViewModel?, string, ResizeMode, bool, bool)"/>
        Task Show(Type typeWindowViewModel,
            CustomWindow customWindow,
            WindowViewModel? viewModel = null,
            string title = "",
            ResizeMode resizeMode = ResizeMode.NoResize,
            bool isDialog = false,
            bool isParent = true);

        /// <inheritdoc cref="Show{TWindowViewModel}(CustomWindow, TWindowViewModel?, string, ResizeMode, bool, bool)"/>
        Task Show(CustomWindow customWindow,
            WindowViewModel? viewModel = null,
            string title = "",
            ResizeMode resizeMode = ResizeMode.NoResize,
            bool isDialog = false,
            bool isParent = true);

        /// <summary>
        /// 显示一个弹窗，返回 <see langword="true"/> 确定(仅当ViewModel继承自<see cref="DialogWindowViewModel"/>时生效)，<see langword="false"/> 取消
        /// </summary>
        /// <typeparam name="TWindowViewModel"></typeparam>
        /// <param name="customWindow"></param>
        /// <param name="viewModel"></param>
        /// <param name="title"></param>
        /// <param name="resizeMode"></param>
        /// <param name="isDialog"></param>
        /// <returns></returns>
        Task<bool> ShowDialog<TWindowViewModel>(
            CustomWindow customWindow,
            TWindowViewModel? viewModel = null,
            string title = "",
            ResizeMode resizeMode = ResizeMode.NoResize,
            bool isDialog = true,
            bool isParent = true)
            where TWindowViewModel : WindowViewModel, new();

        /// <inheritdoc cref="ShowDialog{TWindowViewModel}(CustomWindow, TWindowViewModel?, string, ResizeMode, bool, bool)"/>
        Task ShowDialog(Type typeWindowViewModel,
            CustomWindow customWindow,
            WindowViewModel? viewModel = null,
            string title = "",
            ResizeMode resizeMode = ResizeMode.NoResize,
            bool isDialog = true);

        /// <inheritdoc cref="ShowDialog{TWindowViewModel}(CustomWindow, TWindowViewModel?, string, ResizeMode, bool, bool)"/>
        Task ShowDialog(CustomWindow customWindow,
            WindowViewModel? viewModel = null,
            string title = "",
            ResizeMode resizeMode = ResizeMode.NoResize,
            bool isDialog = true);

        /// <summary>
        /// 根据视图模型关闭窗口
        /// </summary>
        /// <param name="vm"></param>
        void CloseWindow(WindowViewModel vm)
        {

        }

        /// <summary>
        /// 获取视图模型对应的窗口是否显示
        /// </summary>
        /// <param name="vm"></param>
        /// <returns></returns>
        bool IsVisibleWindow(WindowViewModel vm)
        {
            return default;
        }

        /// <summary>
        /// 根据视图模型隐藏窗口
        /// </summary>
        /// <param name="vm"></param>
        void HideWindow(WindowViewModel vm)
        {
        }

        /// <summary>
        /// 根据视图模型显示窗口
        /// </summary>
        /// <param name="vm"></param>
        async void ShowWindow(WindowViewModel vm)
        {
            var windowName = vm.GetType().Name.TrimEnd(nameof(WindowViewModel));
            if (Enum.TryParse<CustomWindow>(windowName, out var customWindow))
            {
                await Show(customWindow, vm);
            }
        }
    }

    /// <inheritdoc cref="IWindowManager"/>
    public interface IWindowManagerImpl : IWindowManager
    {
        Type? WindowType { get; }

        Type GetWindowType(CustomWindow customWindow, params Assembly[]? assemblies)
        {
            IEnumerable<Assembly>? assemblies_ = assemblies;
            return GetWindowType(customWindow, assemblies_);
        }

        Type GetWindowType(CustomWindow customWindow, IEnumerable<Assembly>? assemblies)
        {
            var typeName = $"System.Application.UI.Views.Windows.{customWindow}Window";
            const string errMsg = "GetWindowType fail.";
            return GetType(typeName, WindowType, errMsg, customWindow, assemblies);
        }

        Type GetWindowViewModelType(CustomWindow customWindow, params Assembly[]? assemblies)
        {
            IEnumerable<Assembly>? assemblies_ = assemblies;
            return GetWindowViewModelType(customWindow, assemblies_);
        }

        Type GetWindowViewModelType(CustomWindow customWindow, IEnumerable<Assembly>? assemblies)
        {
            var typeName = $"System.Application.UI.ViewModels.{customWindow}WindowViewModel";
            const string errMsg = "GetWindowViewModelType fail.";
            return GetType(typeName, typeof(WindowViewModel), errMsg, customWindow, assemblies);
        }

        private Type GetType(string typeName, Type? baseType, string errMsg, CustomWindow customWindow, IEnumerable<Assembly>? assemblies)
        {
            Type? type = null;
            if (assemblies.Any_Nullable())
            {
                foreach (var item in assemblies!)
                {
                    type = item.GetType(typeName);
                    if (type != null) break;
                }
            }
            else
            {
                type = Type.GetType(typeName);
            }
            if (type != null && (baseType == null || baseType.IsAssignableFrom(type))) return type;
            throw new ArgumentOutOfRangeException(nameof(customWindow), customWindow, errMsg);
        }
    }
}
