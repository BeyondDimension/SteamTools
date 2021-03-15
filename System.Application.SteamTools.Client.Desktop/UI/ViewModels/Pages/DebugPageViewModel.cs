using Microsoft.Extensions.Options;
using ReactiveUI;
using System.Application.Models;
using System.Application.Repositories;
using System.Application.Services;
using System.Application.UI.Resx;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Security;
using System.Text;
using System.Threading.Tasks;
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
            StringBuilder @string = new();

            @string.AppendFormatLine("Culture: {0}", CultureInfo.CurrentCulture);
            @string.AppendFormatLine("UICulture: {0}", CultureInfo.CurrentUICulture);
            @string.AppendFormatLine("DefaultThreadCulture: {0}", CultureInfo.DefaultThreadCurrentCulture);
            @string.AppendFormatLine("DefaultThreadUICulture: {0}", CultureInfo.DefaultThreadCurrentUICulture);

            @string.AppendFormatLine("UserName: {0}", Environment.UserName);
            @string.AppendFormatLine("MachineName: {0}", Environment.MachineName);
            @string.AppendFormatLine("ApplicationData: {0}", Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData));
            @string.AppendFormatLine("LocalApplicationData: {0}", Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData));
            @string.AppendFormatLine("UserProfile: {0}", Environment.GetFolderPath(Environment.SpecialFolder.UserProfile));
            @string.AppendFormatLine("System: {0}", Environment.GetFolderPath(Environment.SpecialFolder.System));
            @string.AppendFormatLine("SystemX86: {0}", Environment.GetFolderPath(Environment.SpecialFolder.SystemX86));
            @string.AppendFormatLine("Windows: {0}", Environment.GetFolderPath(Environment.SpecialFolder.Windows));

            var dps = DI.Get<IDesktopPlatformService>();
            @string.AppendFormatLine("SteamDirPath: {0}", dps.GetSteamDirPath());
            @string.AppendFormatLine("SteamProgramPath: {0}", dps.GetSteamProgramPath());
            @string.AppendFormatLine("LastSteamLoginUserName: {0}", dps.GetLastSteamLoginUserName());
            (byte[] key, byte[] iv) = dps.MachineSecretKey;
            @string.AppendFormatLine("MachineSecretKey.key: {0}", key.Base64UrlEncode());
            @string.AppendFormatLine("MachineSecretKey.iv: {0}", iv.Base64UrlEncode());

            var stopwatch = Stopwatch.StartNew();

            try
            {
                await TestSecurityStorage();
                @string.AppendLine("TestSecurityStorage: OK");
            }
            catch (Exception e)
            {
                @string.AppendLine("TestSecurityStorage: Error");
                @string.AppendLine(e.ToString());
            }

            finally
            {
                stopwatch.Stop();
                @string.AppendFormatLine("ElapsedMilliseconds: {0}ms", stopwatch.ElapsedMilliseconds);
            }

            var repository = DI.Get<IGameAccountPlatformAuthenticatorRepository>();

            stopwatch.Restart();

            var secondaryPassword = "12345678";
            var value = new GAPAuthenticatorValueDTO.SteamAuthenticator
            {
                DeviceId = "dsafdsaf",
                Serial = "qwewqrwqtr",
                SteamData = "cxzvcxzvcxzv",
                SessionData = "bbbb",
            };
            var item = new GAPAuthenticatorDTO
            {
                Name = "name",
                ServerId = Guid.NewGuid(),
                Value = value,
            };

            try
            {
                await repository.InsertOrUpdateAsync(item, true, secondaryPassword);
                @string.AppendLine("GAPA_Insert: OK");
            }
            catch (Exception e)
            {
                @string.AppendLine("GAPA_Insert: Error");
                @string.AppendLine(e.ToString());
            }
            finally
            {
                stopwatch.Stop();
                @string.AppendFormatLine("ElapsedMilliseconds: {0}ms", stopwatch.ElapsedMilliseconds);
            }

            stopwatch.Restart();

            IGAPAuthenticatorDTO? item2 = null;

            try
            {
                var all = await repository.GetAllAsync(secondaryPassword);
                item2 = all.FirstOrDefault(x => x.Id == item.Id);
                @string.AppendLine("GAPA_GetAll: OK");
            }
            catch (Exception e)
            {
                @string.AppendLine("GAPA_GetAll: Error");
                @string.AppendLine(e.ToString());
            }
            finally
            {
                stopwatch.Stop();
                @string.AppendFormatLine("ElapsedMilliseconds: {0}ms", stopwatch.ElapsedMilliseconds);
            }

            if (item2 == null)
            {
                @string.AppendLine("GAPA_ITEM: NULL");
            }
            else
            {
                if (item2.Name != item.Name)
                {
                    @string.AppendLine("GAPA_ITEM_!=: Name");
                }
                if (item2.ServerId != item.ServerId)
                {
                    @string.AppendLine("GAPA_ITEM_!=: ServerId");
                }
                if (item2.Value is GAPAuthenticatorValueDTO.SteamAuthenticator value2)
                {
                    if (value2.DeviceId != value.DeviceId)
                    {
                        @string.AppendLine("GAPA_ITEM_!=: Value.DeviceId");
                    }
                    if (value2.Serial != value.Serial)
                    {
                        @string.AppendLine("GAPA_ITEM_!=: Value.Serial");
                    }
                    if (value2.SteamData != value.SteamData)
                    {
                        @string.AppendLine("GAPA_ITEM_!=: Value.SteamData");
                    }
                    if (value2.SessionData != value.SessionData)
                    {
                        @string.AppendLine("GAPA_ITEM_!=: Value.SessionData");
                    }
                }
                else
                {
                    @string.AppendLine("GAPA_ITEM_!=: Value");
                }
            }

            try
            {
                await repository.DeleteAsync(item.ServerId.Value);
                @string.AppendLine("GAPA_Delete: OK");
            }
            catch (Exception e)
            {
                @string.AppendLine("GAPA_Delete: Error");
                @string.AppendLine(e.ToString());
            }

            var options = DI.Get<IOptions<AppSettings>>();
            string embeddedAes;
            try
            {
                embeddedAes = options.Value.Aes.ToString();
            }
            catch (Exception e)
            {
                embeddedAes = e.ToString();
            }
            @string.AppendFormatLine("EmbeddedAes: {0}", embeddedAes);

            DebugString = @string.ToString();
        }

        public async void DebugButton_Click2()
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

        static async Task TestSecurityStorage()
        {
            await IStorage.Instance.SetAsync("↑↑", Encoding.UTF8.GetBytes("↓↓"));

            var left_top_ = await IStorage.Instance.GetAsync<byte[]>("↑↑");

            var left_top = Encoding.UTF8.GetString(left_top_.ThrowIsNull("↑-key"));

            if (left_top != "↓↓")
            {
                throw new Exception("↓↓");
            }

            await IStorage.Instance.SetAsync<string>("←←", "→→");

            var left_left = await IStorage.Instance.GetAsync<string>("←←");

            if (left_left != "→→")
            {
                throw new Exception("→→");
            }

            await IStorage.Instance.SetAsync("aa", "bb");

            var left_aa = await IStorage.Instance.GetAsync("aa");

            if (left_aa != "bb")
            {
                throw new Exception("bb");
            }

            var dict = new Dictionary<string, string> {
                { "🎈✨", "🎆🎇" },
                { "✨🎊", "🎃🎑" },
            };

            await IStorage.Instance.SetAsync("dict", dict);

            var left_dict = await IStorage.Instance.GetAsync<Dictionary<string, string>>("dict");

            if (left_dict == null)
            {
                throw new Exception("dict is null.");
            }

            if (left_dict.Count != dict.Count)
            {
                throw new Exception("dict Count !=.");
            }
        }
    }
}