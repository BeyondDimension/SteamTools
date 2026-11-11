# SteamTools (Watt Toolkit) — NixOS 优化适配分支

> **适配版 v0.0.1**（基于上游 3.1.0）。**System 加速模式暂不可用**（依赖 GNOME gsettings 系统代理，NixOS 下不适用），仅支持 `hosts` / `dns` 两种模式。

本项目是 **SteamTools（Watt Toolkit，原 Steam++）官方源码的 NixOS 适配分支**，解决不可变系统（NixOS）下的三个核心问题：

1. **声明式打包**：`flake.nix` + `package.nix` 提供 `out`（主程序 GUI）/ `accelerator`（加速器系统服务）两个输出；根证书不再随包分发，由程序首次启动时在 AppData 生成、再导出到可写路径供声明式信任；
2. **系统级加速器服务**：加速器不以子进程随 GUI 启动，由 `programs.watt-toolkit` 模块配置的 systemd 服务运行（空闲，UI 点击时拉起），`AmbientCapabilities` 注入 `cap_net_bind_service` 等，普通用户即可监听 443，无需 root / fhsenv / setcap；
3. **版本锁定 + CI 自动发布**：`flake.lock` 锁定 nixpkgs revision；GitHub Actions 每次构建成功后自动上传 Release 并**回填 `package.nix` 的 url / sha256**（运行版本与构建版本一致，防漂移）。

## 分支与文件

| 路径 | 说明 |
| --- | --- |
| `nixos-branch` | 官方源码 + NixOS 适配补丁 + CI workflow |
| `steamtools-nixos-branch.patch` | NixOS 适配补丁（`git apply` 可应用） |
| `flake.nix` / `flake.lock` | 可直接 import 的 flake（包 + NixOS 模块 + 开发环境） |
| `package.nix` | 包定义（url/sha256 由 CI 自动回填） |
| `module.nix` | `programs.watt-toolkit` 模块（服务/证书/polkit） |
| `.github/workflows/nixos-branch-build.yml` | CI：构建发布 linux-x64 → 上传 Release → 回填 url/sha256 |

## NixOS 使用（直接 import 本 flake）

在 nixos-configuration 中：

```nix
{
  inputs = {
    nixpkgs.url = "github:NixOS/nixpkgs/nixos-unstable";
    steamtools = {
      url = "github:Aozora-Wings/SteamTools-nixos/nixos-branch";
      # 国内网络可换镜像源（与 nixpkgs 同法）：
      # url = "git+https://mirrors.nju.edu.cn/git/SteamTools-nixos.git?ref=nixos-branch";
    };
  };

  outputs = { nixpkgs, steamtools, ... }: {
    nixosConfigurations.myhost = nixpkgs.lib.nixosSystem {
      modules = [
        steamtools.nixosModules.default
        ({ pkgs, ... }: {
          programs.watt-toolkit = {
            enable = true;
            # 默认已指向 steamtools.packages.${pkgs.system}.default，可显式覆盖：
            # package = steamtools.packages.${pkgs.system}.default;
            user = "wt";                       # 加速器服务运行用户（不能是 root）
            enableAcceleratorService = true;   # 系统级加速器服务（空闲，UI 点击时拉起）
            trustCertificate = true;           # security.pki.certificates 读取运行时导出的 /var/lib/watt-toolkit/*.cer
            # trustFirefoxCertificate = true;   # Firefox 信任系统最终证书集合（Install 系统 CA 合并库 /etc/ssl/certs/ca-certificates.crt，默认 true）
            enablePolkit = true;               # 允许主用户无密码启停加速器服务
          };
        })
      ];
    };
  };
}
```

### 调用例子（简写）

```nix
# 只装软件（不带服务）：
environment.systemPackages = [ steamtools.packages.${pkgs.system}.default ];

# 只用模块（含加速器服务 + 证书信任 + polkit）：
imports = [ steamtools.nixosModules.watt-toolkit ];
programs.watt-toolkit.enable = true;

# 旧名兼容：
services.steamtools.enable = true;   # 映射到同一配置
```

### ⚠️ 构建 / 切换必须加 `--impure`

`programs.watt-toolkit` 在**求值阶段**读取本机由程序运行时生成的证书
（`/var/lib/watt-toolkit/SteamTools.Certificate.cer`），再拼进 `security.pki`——
这是 flake 之外的本地状态，纯净求值会直接报错拒绝构建。所有命令都要带 `--impure`：

```bash
sudo nixos-rebuild switch --impure --flake .#myhost
```

首次部署的顺序（证书是运行时才有的，构建期不能凭空产生）：

1. `sudo nixos-rebuild switch --impure --flake .#myhost` —— 先装好程序和加速器服务；
2. 启动 GUI（`watt-toolkit`），点一次「加速」，程序会在 AppData 生成 CA 并导出到
   `/var/lib/watt-toolkit/SteamTools.Certificate.cer`；
3. 再执行一次同一条 `--impure` 重建 —— 这次证书才会被拼进
   `/etc/ssl/certs/ca-certificates.crt`，加速才能真正跑起来。

程序若检测到证书尚未进入系统信任库，会直接拦下加速并提示需要再次 rebuild。

## 加速模式（proxyMode）

NixOS 下加速模式由系统配置统一管理（**UI 中 Hosts / System / DNS 三个模式选项为灰色只读**，
鼠标悬停提示去系统配置修改；UI 会显示当前系统配置的模式）。**System 模式暂不可用**：其实现依赖
GNOME gsettings（`org.gnome.system.proxy`），与 NixOS 声明式理念及非 GNOME 桌面（niri/KDE 等）不兼容，
故只开放 `hosts` / `dns` 两种模式。模块选项：

```nix
programs.watt-toolkit = {
  enable = true;
  proxyMode = "hosts";   # "hosts"（默认）| "dns"
};
```

| 模式 | 机制 | 适用 |
| --- | --- | --- |
| `hosts`（默认） | 静态域名劫持：域名列表由本模块/系统配置静态写入，加速器监听 443 做 HTTPS 中间人 | 无额外常驻依赖；**当前静态列表仅覆盖 Steam 平台域名**（17 个），其他平台（GOG/Epic 等）域名未包含，需在 hosts 配置中补充 |
| `dns` | 全局 dnsmasq 动态劫持：dnsmasq 作为系统解析器（`networking.nameservers=127.0.0.1`，NetworkManager `dns=none` 禁用 DHCP 下发 DNS），UI 点击加速时把**选中平台的完整域名列表**动态写入 `/run/dnsmasq.d/steampp.conf`（`address=/域名/127.0.0.1`）并 restart；停止加速自动撤销恢复直连 | 域名列表随选中平台动态变化，接口无关（多 WiFi/有线通用）；代价是 dnsmasq 常驻（systemd 托管，`Restart=on-failure`） |

### dns 模式细节

- 主程序加速启动时把选中平台的监听域名列表写入 `/tmp/steampp-domains.conf`；
- 加速器服务 `ExecStartPost` 联动 root oneshot 服务 `watt-toolkit-dns-update`，
  把域名列表转换为 dnsmasq `address=/域名/127.0.0.1` 规则写入 `/run/dnsmasq.d/steampp.conf` 并 `systemctl restart dnsmasq`（SIGHUP reload 不会重新扫描 conf-dir）；
- `ExecStopPost` 撤销映射（清空 `steampp.conf` 并 restart），选中域名恢复上游直连；
- dnsmasq 上游：`223.5.5.5 / 119.29.29.29 / 8.8.8.8`（选中域名以外所有查询直接上游解析，不经过加速器）。

### hosts 模式注意

当前静态 hosts 列表**只包含 Steam 平台域名**（`steam-hosts.nix`：steamcdn/steamstatic/steampowered 等 17 个域名）。
其他平台加速（GOG/Epic/Origin/Uplay 等）的域名**未写入系统 hosts**——如使用 hosts 模式加速其他平台，
需自行补充对应平台域名到系统配置，或改用 `dns` 模式（自动覆盖全部选中平台域名）。

## 输出说明

| 输出 | 内容 | 用途 |
| --- | --- | --- |
| `out` | 主程序（watt-toolkit GUI，wrapper 注入运行时） | `environment.systemPackages` |
| `accelerator` | 加速器模块（目录发布，含 `Steam++.Accelerator.dll`） | systemd 服务 `ExecStart` |
| `ssl` | openssl 生成的系统根证书（cer + pfx） | `security.pki.certificateFiles` / 应用启动同步 AppData |

## CI 自动回填（url / sha256）

`.github/workflows/nixos-branch-build.yml` 构建成功后自动：

1. 计算 tgz 的 sha256；
2. 上传到 GitHub Release（固定 tag `nixos-build`，固定文件名 `Steam++_linux_x64_nixos.tgz`，URL 稳定）；
3. 回填 `package.nix` 的 `publishUrl` / `publishSha256`；
4. `[skip ci]` 提交推送（不触发循环构建）。

**前置要求**：仓库 Actions 需要 **Read and write** 权限（`Settings → Actions → General → Workflow permissions → Read and write permissions`），否则 Release 上传 / 回填提交会失败。

## 开发测试环境

```sh
nix develop        # 进入（.NET SDK 11 preview + 构建/发布工具链）
git apply ./steamtools-nixos-branch.patch
dotnet build src/BD.WTTS.Client.Tools.Publish -c Release -t:Rebuild -p:UseSharedCompilation=false
cd src/BD.WTTS.Client.Tools.Publish/bin/Release/net11.0
dotnet pub.dll run --rids linux-x64
```

## 许可证

上游项目为 GPL-3.0-only，本分支同样遵循（见 `LICENSE` / 上游 [BeyondDimension/SteamTools](https://github.com/BeyondDimension/SteamTools)）。
