using System;
using System.Application.UI.Resx;
using System.Collections.Generic;
using System.Text;
using System.Windows.Input;
using ReactiveUI;

// ReSharper disable once CheckNamespace
namespace System.Application.UI.ViewModels
{
    public partial class StartPageViewModel
    {
        public class FeatureItem
        {
            public FeatureItem()
            {
                InvokeCommand = ReactiveCommand.Create<object>(OnInvokeCommandExecute);
            }

            public string? Header { get; set; }

            public string? Description { get; set; }

            public string? PreviewImageSource { get; set; }

            public string? PageType { get; init; }

            public ICommand InvokeCommand { get; }

            private void OnInvokeCommandExecute(object parameter)
            {
            }
        }

        public class FeatureGroup
        {
            public string? Header { get; set; }

            public List<FeatureItem>? Items { get; init; }
        }

        public StartPageViewModel()
        {
            FeatureGroups = new()
            {
                new()
                {
                    Header = "最新公告 🎉~",
                    Items = new()
                    {
                        new()
                        {
                            Header = "今日免费游戏领取信息",
                            Description = "2022-03-24",
                        },
                        new()
                        {
                            Header = "修复通知",
                            Description = "近期xx功能失效问题已经修复",
                        },
                    },
                },
                new()
                {
                    Header = "新功能上线 🆕~",
                    Items = new()
                    {
                        new()
                        {
                            Header = "Steam库存游戏编辑",
                            Description = "支持编辑Steam的游戏名称、图片、启动项等信息并且保存到Steam客户端内也会生效",
                        },
                        new()
                        {
                            Header = "Steam库存游戏编辑",
                            Description = "支持编辑Steam的游戏名称、图片、启动项等信息并且保存到Steam客户端内也会生效",
                        },
                        new()
                        {
                            Header = "Steam库存游戏编辑",
                            Description = "支持编辑Steam的游戏名称、图片、启动项等信息并且保存到Steam客户端内也会生效",
                        },
                        new()
                        {
                            Header = "Steam库存游戏编辑",
                            Description = "支持编辑Steam的游戏名称、图片、启动项等信息并且保存到Steam客户端内也会生效",
                        },
                        new()
                        {
                            Header = "Steam库存游戏编辑",
                            Description = "支持编辑Steam的游戏名称、图片、启动项等信息并且保存到Steam客户端内也会生效",
                        },
                    },
                },
                new()
                {
                    Header = "已有功能 💖~",
                    Items = new()
                    {
                        new()
                        {
                            Header = "Steam下载完成定时关机",
                            Description = "可以解决想挂机更新下载游戏Steam自身却没有定时关机的痛点问题，程序会监控到指定的Steam游戏下载完成后自动执行关机睡眠等操作",
                        },
                        new()
                        {
                            Header = "Steam家庭库共享管理",
                            Description = "支持排序或临时禁用某个账号的共享来解决多个号同时共享一个游戏的却总是被一个号占用导致无法游玩共享游戏",
                        },
                    },
                },
            };

        }


        public List<FeatureGroup> FeatureGroups { get; }
    }
}
