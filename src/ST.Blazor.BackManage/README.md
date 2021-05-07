# **次元超越**平台内部后台管理系统
使用 [Ant Design Blazor](https://github.com/ant-design-blazor/ant-design-blazor) 构建的企业级产品，创造高效愉悦的工作体验。

## ✨ 特性

- 🌈 提炼自企业级中后台产品的交互语言和视觉风格。
- 📦 开箱即用的高质量 Blazor 组件，可在多种托管方式共享。
- 💕 支持基于 WebAssembly 的客户端和基于 SignalR 的服务端 UI 事件交互。
- 🎨 支持渐进式 Web 应用（PWA）
- 🛡 使用 C# 构建，多范式静态语言带来高效的开发体验。
- ⚙️ 基于 .NET Standard 2.1 / .NET 5，可直接引用丰富的 .NET 类库。
- 🎁 可与已有的 ASP.NET Core MVC、Razor Pages 项目无缝集成。

## 🌐 [ASP.NET Core Blazor 简介](https://docs.microsoft.com/zh-cn/aspnet/core/blazor/?view=aspnetcore-5.0)
Blazor 是一个使用 [.NET](https://docs.microsoft.com/zh-cn/dotnet/standard/tour) 生成交互式客户端 Web UI 的框架：

* 使用 [C#](https://docs.microsoft.com/zh-cn/dotnet/csharp/) 代替 [JavaScript](https://www.javascript.com) 来创建信息丰富的交互式 UI。
* 共享使用 .NET 编写的服务器端和客户端应用逻辑。
* 将 UI 呈现为 HTML 和 CSS，以支持众多浏览器，其中包括移动浏览器。
* 与新式托管平台（如 [Docker](https://docs.microsoft.com/zh-cn/dotnet/standard/microservices-architecture/container-docker-introduction/index)）集成。

使用 .NET 进行客户端 Web 开发可提供以下优势：

* 使用 C# 代替 JavaScript 来编写代码。
* 利用现有的 [.NET 库](https://docs.microsoft.com/zh-cn/dotnet/standard/class-libraries)生态系统。
* 在服务器和客户端之间共享应用逻辑。
* 受益于 .NET 的性能、可靠性和安全性。
* 在 Windows、Linux 和 macOS 上使用 [Visual Studio](https://visualstudio.microsoft.com) 保持高效工作。
* 以一组稳定、功能丰富且易用的通用语言、框架和工具为基础来进行生成。

## 🖥 支持环境

- .NET Core 3.1 / .NET 5。
- Blazor WebAssembly 3.2 /.NET 5 正式版。
- 支持服务端双向绑定。
- 支持 WebAssembly 静态文件部署。
- 主流 4 款现代浏览器，以及 Internet Explorer 11+（限 [Blazor Server](https://docs.microsoft.com/en-us/aspnet/core/blazor/supported-platforms?view=aspnetcore-3.1&WT.mc_id=DT-MVP-5003987)）。
- 可直接运行在 [Electron](http://electron.atom.io/) 等基于 Web 标准的环境上。

| [<img src="https://cdn.jsdelivr.net/gh/alrra/browser-logos/src/edge/edge_48x48.png" alt="IE / Edge" width="24px" height="24px" />](http://godban.github.io/browsers-support-badges/)</br> Edge / IE | [<img src="https://cdn.jsdelivr.net/gh/alrra/browser-logos/src/firefox/firefox_48x48.png" alt="Firefox" width="24px" height="24px" />](http://godban.github.io/browsers-support-badges/)</br>Firefox | [<img src="https://cdn.jsdelivr.net/gh/alrra/browser-logos/src/chrome/chrome_48x48.png" alt="Chrome" width="24px" height="24px" />](http://godban.github.io/browsers-support-badges/)</br>Chrome | [<img src="https://cdn.jsdelivr.net/gh/alrra/browser-logos/src/safari/safari_48x48.png" alt="Safari" width="24px" height="24px" />](http://godban.github.io/browsers-support-badges/)</br>Safari | [<img src="https://cdn.jsdelivr.net/gh/alrra/browser-logos/src/opera/opera_48x48.png" alt="Opera" width="24px" height="24px" />](http://godban.github.io/browsers-support-badges/)</br>Opera | [<img src="https://cdn.jsdelivr.net/gh/alrra/browser-logos/src/electron/electron_48x48.png" alt="Electron" width="24px" height="24px" />](http://godban.github.io/browsers-support-badges/)</br>Electron |
| :-------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------: | :--------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------: | :----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------: | :----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------: | :------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------: | :------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------: |
|                                                                                          Edge 16 / IE 11†                                                                                           |                                                                                                 52                                                                                                  |                                                                                                57                                                                                                |                                                                                                11                                                                                                |                                                                                              44                                                                                              |                                                                                               Chromium 57                                                                                                |

> 由于 [WebAssembly](https://webassembly.org) 的限制，Blazor WebAssembly 不支持 IE 浏览器，但 Blazor Server 支持 IE 11†。 详见[官网说明](https://docs.microsoft.com/en-us/aspnet/core/blazor/supported-platforms?view=aspnetcore-3.1&WT.mc_id=DT-MVP-5003987)。

## 🚀 热重载
dotnet watch run debug
