# Accelerator Module - 网络加速模块

这是从Watt Toolkit项目中提取的网络加速模块的精简版本。

## 项目概述

本模块专注于网络加速功能，包含以下核心组件：

### 主要项目

1. **BD.WTTS.Client** - 核心客户端库
   - 基础服务和实体
   - 插件系统基础设施
   - 通用工具和帮助类

2. **BD.WTTS.Client.Avalonia** - UI框架层
   - Avalonia UI集成
   - 跨平台桌面界面支持
   - 用户界面基础组件

3. **BD.WTTS.Client.IPC** - 进程间通信
   - 客户端间通信机制
   - 服务接口定义
   - 数据传输协议

4. **BD.WTTS.Client.Plugins.Accelerator** - 网络加速插件
   - 游戏加速服务
   - 代理服务器管理
   - 加速规则配置
   - XunYou SDK集成

5. **BD.WTTS.Client.Plugins.Accelerator.ReverseProxy** - 反向代理服务
   - YARP反向代理实现
   - DNS拦截和分析
   - SSL证书管理
   - 流量统计和监控

### 核心功能

- **网络加速**: 通过反向代理技术优化网络连接
- **DNS拦截**: 智能DNS分析和域名解析优化
- **代理服务**: 支持多种代理模式和外部代理类型
- **流量监控**: 实时网络流量统计和分析
- **证书管理**: 自动SSL证书生成和管理
- **游戏加速**: 针对游戏的网络优化服务

### 技术栈

- **.NET 7**: 跨平台开发框架
- **Avalonia**: 跨平台UI框架
- **YARP**: 微软反向代理框架
- **ReactiveUI**: 响应式UI框架
- **NLog**: 日志记录框架
- **SQLite**: 本地数据存储

### 支持的平台

- Windows 10/11
- macOS 10.15+
- Linux (Ubuntu 18.04+)

## 构建要求

- Visual Studio 2022 或 JetBrains Rider
- .NET 7 SDK
- Git

## 开发说明

### 项目结构

```
src/
├── BD.WTTS.Client/                    # 核心客户端库
├── BD.WTTS.Client.Avalonia/          # UI框架层
├── BD.WTTS.Client.IPC/               # 进程间通信
├── BD.WTTS.Client.Plugins.Accelerator/ # 加速插件
├── BD.WTTS.Client.Plugins.Accelerator.ReverseProxy/ # 反向代理服务
├── ImplicitUsings/                   # 隐式导入
├── XunYouSDK/                       # 迅游SDK集成
└── TFM_*.props                      # 目标框架配置
```

### 构建命令

```bash
# 构建解决方案
dotnet build Accelerator.sln

# 运行反向代理服务
dotnet run --project src/BD.WTTS.Client.Plugins.Accelerator.ReverseProxy
```

### 配置说明

主要配置文件位于各项目的`Settings`目录中，包括：
- 代理设置 (`ProxySettings`)
- 游戏加速设置 (`GameAcceleratorSettings`)
- 网络配置参数

## 许可证

遵循原项目的开源许可证条款。

## 贡献

欢迎提交Issue和Pull Request来帮助改进这个网络加速模块。