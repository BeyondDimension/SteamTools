# 前言
Steam++项目创建于2018年5月8日，不过并不是耗时两年才做出来，之前一直由于工作原因，并没有多少空闲时间，直到今年中旬辞职了，才开始猛肝。
本来是想一直做到完全体了，才考虑发布，不过现在感慨一下，果然是没法这么快就整完的，所以还是一版一版更新做到完全体吧。
Steam++只在keylol论坛与github发布，为了账号安全，请不要到其它不明地方下载。

# Steam++工具箱

## 已知的未修复问题
* 部分用户无法加载游戏列表，原因可能是因为第一次加载游戏列表时未加载完成程序停止运行了，导致生成了错误的缓存文件apps.json，如果遇到此问题可尝试手动删除程序目录下的apps.json然后重启程序。


## 更新内容

### 1.0.4
```
 此版本修改了配置文件的保存读取功能，会无法读取1.0.4版本以前的配置文件包括令牌数据。(可以通过设置-令牌设置-导入旧版本Steam++令牌数据按钮来恢复旧版本令牌数据)
 还原steam社区反代上游地址修复部分请求会提示需要登录的问题
 新增令牌导入导出备份功能
 新增令牌可以选择保存在程序根目录下的设置选项(方便备份)
 修改了自动更新的提示方式
 自动更新可以自动覆盖升级，不再需要手动替换
 修复令牌无法保存的问题
```

### 1.0.3.3
```
新增pixiv本地反代服务支持
新增在欢迎页steam昵称后显示steam的登陆区域
添加steam图片修复的本地反代支持
修复最小化恢复时UI边框错位的BUG
修复令牌编辑和导入时出现的非空报错
帐户切换功能的最近登陆时间从北京时间改为当前系统时区时间
修复一个因为DNS解析错误会导致程序内存溢出闪退的BUG
SDA令牌导入不再支持导入加密文件，如果要导入加密的maFile请先在SDA移除加密
修改初始化的方式和steam api的连接逻辑来避免steam游戏掉帧的BUG
解决程序最小化启动时弹窗报错
优化本地令牌的加载
修复托盘图标单击启动多个窗口
修复设置界面设置UI错位
修复成就窗口关闭游戏依然运行
修改steam社区商店图片代理上游地址为steampowered.com
```

### 1.0.2

```
更新了由Benares制作的图标
新增帐户切换删除功能
添加github图片资源文件代理服务与discord代理服务支持
因为有问题暂时去掉了GOG Galaxy代理选项
修复一个会导致程序启动没有反应的bug
修复托盘菜单steam昵称过长ui错位bug
新增steam启动参数设置
新增启动时程序最小化到托盘菜单设置
新增捐助列表的展示
新增工具自动更新功能
```

## 工具介绍

   `Steam++`是一个包含多种Steam工具功能的工具箱，开源发布于[Github](https://github.com/rmbadmin/SteamTools)，如果您对发布的二进制文件不放心，可以自行下载源码编译运行。
   此工具的大部分功能都是需要您下载安装Steam才能使用。
   工具预计将整合进大部分常用的Steam相关工具，并且尽力做到比原工具更好用，在各种整合添加功能的同时，也会注意体积尽量的控制到最小。

软件截图
[hide]
[attachimg]1228394[/attachimg]
[attachimg]1228395[/attachimg]
[attachimg]1228396[/attachimg]
[attachimg]1228397[/attachimg]
[/hide]

## 核心功能


### 1. 反代Steam的社区网页使其能正常访问

 功能类似羽翼城大佬的[steamcommunity_302](https://www.dogfight360.com/blog/686/),使用[Titanium-Web-Proxy](https://github.com/justcoding121/Titanium-Web-Proxy)开源项目进行本地反代，相比302工具具有更快的启动速度，以及支持简单的脚本注入。该功能也可以配合羽翼城大佬的[UsbEAm Hosts Editor](https://www.dogfight360.com/blog/475/)里的网页相关-steamcommunity_302 社区/api/商店加载速度选项的hosts提升加载速度。
[attachimg]1228388[/attachimg]

### 2. 快速切换当前PC已经记住登陆的Steam账号

该功能是读取Steam路径下存储的本地用户登录记录直接展示操作，可以多账号切换无需重新输入密码和令牌。
[attachimg]1228389[/attachimg]

### 3. Steam游戏的成就统计管理功能

 功能参考[SteamAchievementManager](https://github.com/gibbed/SteamAchievementManager)进行二次开发，修复了成就语言有中文却依然是英文成就信息的BUG，修改了游戏列表的加载和操作易用性。
[attachimg]1228390[/attachimg]

### 4. Steam本地两步身份验证器

功能参考[WinAuth](https://github.com/winauth/winauth)开发，可以使您不用启动移动版Steam App也能查看您的令牌，功能类似的软件有[WinAuth](https://github.com/winauth/winauth)、[SteamDesktopAuthenticator](https://github.com/Jessecar96/SteamDesktopAuthenticator)。
[attachimg]1228391[/attachimg]

### 5. 一些游戏工具

目前已有强制游戏无边框窗口化，CSGO修复VAC屏蔽。
这一块是随缘做一些我经常用或者闲着没事捣鼓的功能。
将任何游戏强制无边框窗口化
[attachimg]1228392[/attachimg]
使任何窗口化游戏变成动态桌面壁纸（终于可以用《山》当壁纸了）
[attachimg]1228393[/attachimg]

------


## 预计后续添加的功能


### Steam自动挂卡

尝试用社区反代功能结合成就解锁功能来重新实现，目的是实现在软件内不用登录Steam即可直接获取徽章卡片信息并开始挂卡。

### Steam皮肤设计器

挖坑画大饼，可视化编辑Steam皮肤，而且如果软件能上架Steam的话打算支持创意工坊分享设计的Steam皮肤，短期内肯定做不完。

### 插件形式加载运行ASF

以插件形式支持ASF在工具内运行并增强ASF在Windows Desktop环境下的使用。

### Steam自定义封面管理

 增强Steam自定义封面的管理以及从[SteamGridDB](https://www.steamgriddb.com/)快速匹配下载应用封面。

------


## 运行环境

> 程序使用C# WPF在 .NET Framework4.7.2环境下开发，如果无法运行请下载安装[.NET Framework 4.7.2](https://dotnet.microsoft.com/download/dotnet-framework/net472)。

## 下载

> [Github](https://github.com/rmbadmin/SteamTools/releases)  
> 分流下载：  
> [hide]
> [蓝奏云](https://wws.lanzous.com/iW1HXjt3vxa)  
> [百度云](https://pan.baidu.com/s/1XHrrBZpdA9orFpMSHT-pRw )  
> 提取码：fwe2
> [/hide]  
> EXE 大小：4.85MB  
> MD5：A6349646509528F66BF9109D315FB5B2
> [查毒链接](https://www.virustotal.com/gui/file/aa2e2d17abc1557a4614aac58d67212ad959d67d725ebeadb23e922077bf82d9/detection)
   虽然没有什么杀毒软件报毒，但是使用过程中可能遇到windows defender误报，您可以选择添加信任。