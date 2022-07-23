using System.Application.Models;
using System.Application.UI.ViewModels;
using System.Threading.Tasks;

namespace System.Application.Services;

partial interface IWindowManager
{
    /// <summary>
    /// 根据平台差异
    /// <para><see langword="true"/>使用 <see cref="MyAuthenticatorWrapper"/> 包装传递 <see cref="MyAuthenticator"/></para>
    /// <para><see langword="false"/>使用具体的窗口视图模型如 <see cref="ShowAuthWindowViewModel"/>，<see cref="AuthTradeWindowViewModel"/></para>
    /// </summary>
    bool UseMyAuthenticatorWrapper => false;

    Task ShowAuth(MyAuthenticator auth)
    {
        const CustomWindow window = CustomWindow.ShowAuth;
        WindowViewModel viewModel = UseMyAuthenticatorWrapper ?
            new MyAuthenticatorWrapper(auth) :
            new ShowAuthWindowViewModel(auth);
        return Show(window, viewModel, resizeMode: ResizeMode.CanResize);
    }

    Task ShowAuthTrade(MyAuthenticator auth)
    {
        const CustomWindow window = CustomWindow.AuthTrade;
        WindowViewModel viewModel = UseMyAuthenticatorWrapper ?
            new MyAuthenticatorWrapper(auth) :
            new AuthTradeWindowViewModel(auth);
        return Show(window, viewModel, resizeMode: ResizeMode.CanResize);
    }
}
