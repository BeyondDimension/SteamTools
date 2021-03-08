using ReactiveUI;
using System.Application.Services;
using System.Application.UI.Resx;
using System.Collections.Generic;
using System.Linq;
using System.Windows;

namespace System.Application.UI.ViewModels
{
    public class DebugPageViewModel : TabItemViewModel
    {
        public override string Name
        {
            get => "Debug";
            protected set { throw new NotImplementedException(); }
        }

        private string _DebugString = string.Empty;
        public string DebugString
        {
            get => _DebugString;
            protected set => this.RaiseAndSetIfChanged(ref _DebugString, value);
        }

        public DebugPageViewModel()
        {

        }

        public async void DebugButton_Click()
        {
            Services.ToastService.Current.Notify("Test CommandTest CommandTest CommandTest CommandTest CommandTest CommandTest CommandTest CommandTest CommandTest CommandTest CommandTest CommandTest CommandTest CommandTest CommandTest CommandTest CommandTest Command");
            DebugString += Services.ToastService.Current.Message + Environment.NewLine;
            DebugString += Services.ToastService.Current.IsVisible + Environment.NewLine;

            var r = await MessageBoxCompat.ShowAsync(@"Steam++ v1.1.2   2021-01-29
更新内容
1、新增账号切换的状态栏右下角登录新账号功能
2、新增实时刷新获取Steam新登录的账号数据功能
3、新增FAQ常见问题疑难解答文本，可以在关于中找到它
4、优化配置文件备份机制，如果配置文件出错会提示还原上次读取成功的配置
5、优化错误日志记录，现在它更详细了
6、修复谷歌验证码代理方式为全局跳转recatpcha
7、修复配置文件加载时提示根元素错误
8、修复某些情况下开机自启失效问题", "Title", MessageBoxButtonCompat.OKCancel);

            DebugString += r + Environment.NewLine;

            //.ContinueWith(s => DebugString += s.Result + Environment.NewLine);

            //            var result = DI.Get<IMessageWindowService>().ShowDialog(@"Steam++ v1.1.2   2021-01-29
            //更新内容
            //1、新增账号切换的状态栏右下角登录新账号功能
            //2、新增实时刷新获取Steam新登录的账号数据功能
            //3、新增FAQ常见问题疑难解答文本，可以在关于中找到它
            //4、优化配置文件备份机制，如果配置文件出错会提示还原上次读取成功的配置
            //5、优化错误日志记录，现在它更详细了
            //6、修复谷歌验证码代理方式为全局跳转recatpcha
            //7、修复配置文件加载时提示根元素错误
            //8、修复某些情况下开机自启失效问题", "Title",true).ContinueWith(s => DebugString += s.Result + Environment.NewLine);
        }
    }
}