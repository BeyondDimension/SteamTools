# SteamTools (Watt Toolkit) —— NixOS 不可变系统适配包
#
# 架构：
#   outputs = [ out accelerator ]
#     out        : 主程序（watt-toolkit，GUI + 插件 UI），Wrapper 注入运行时
#     accelerator: 完整加速器模块（目录发布），供 systemd 系统服务运行
#
# 设计要点（NixOS 不可变特性）：
#   1. 加速器不以子进程方式随 GUI 启动，由系统级 systemd 服务（空闲，UI 点击时
#      systemctl start 拉起）运行，AmbientCapabilities 注入 cap_net_bind_service 等，
#      以普通用户身份监听 443，无需 root / fhsenv / setcap（capset 在 NixOS 返回 EPERM）。
#   2. 证书不在打包期生成（打包期生成会把私钥放进共享、全局可读的 store，且与程序
#      实际使用的私钥不是同一对）。程序启动加速器时惰性生成到 AppData，再把 .cer
#      导出到模块声明的可写路径（STEAMTOOLS_BUNDLED_CER），由 security.pki.certificates
#      声明式读取；私钥始终留在 AppData。
#   3. /etc/hosts 由声明式配置管理（networking.hosts），运行时不可写；加速器的 Hosts
#      条目在 NixOS 分支下写入 AppData 供合并进系统配置。
#   4. 入口 wrapper 直接运行 dotnet + Steam++.dll（目录发布），不依赖 fhsenv。
#
# ⚠️ glibc 混用问题（2026-10-09 实锤）：
#   runtimeLibs 必须【不含 glibc】。此前 makeWrapper --prefix LD_LIBRARY_PATH 注入
#   ${lib.makeLibraryPath runtimeLibs}（含 glibc/lib）：该环境变量被 Watt fork 的
#   子进程继承，系统升级（glibc 2.42→2.44）后旧包未重建时 LD_LIBRARY_PATH 仍钉旧
#   glibc 路径 → systemctl 以新 ld.so 加载旧 libc.so.6 → 混合版本初始化
#   stack smash（SIGABRT）。去掉 glibc 后：Watt 用系统默认 glibc（ld.so 自动解析，
#   已实测 2.44 兼容，无 coredump），fork 出的 systemctl 也用系统纯净 glibc。
#   （ld.so --library-path exec 方案不可行：.NET apphost 检查 /proc/self/exe，
#   直接 exec ld-linux 会报 "cannot execute dotnet when renamed to ld-linux..."。）
#
# 版本来源说明：
#   默认 src = 官方 GitHub Releases 3.1.0 linux_x64 tgz（sha256 固定，可复现）。
#   ⚠️ 官方 release 是「纯净版」，不包含 NixOS 分支适配（服务式加速器 / 证书 store 直读 /
#      hosts dump）。因此：
#     - GUI 主程序：可用（wrapper 注入运行时）。
#     - 加速器系统服务（accelerator 输出）：官方包若为 SingleFile 形态，需要分支发布
#       （目录发布 + .dll）才能由 systemd 的 dotnet 直接运行；分支适配经上游合并并发布
#       新版本后，把下方 url/sha256 更新为新官方包即可。
#   自用分支构建（含全部 NixOS 适配）可覆盖参数：
#     pkgs.callPackage ./package.nix {
#       src = pkgs.fetchurl { url = "https://oazcc.qzapp.qkzy.net/Steam++.tgz"; sha256 = "..."; };
#     }
#   或在 flake 层 --override-input 替换。

{ pkgs
, stdenv
, lib
, src ? null
, sha256 ? null
, ...
}:

let
  # 官方发布包源（nixpkgs 提交/上游测试路径；可被 src/sha256 参数覆盖）
  publishUrl = "https://github.com/Aozora-Wings/SteamTools-nixos/releases/download/v0.0.1/Steam++_linux_x64_nixos_v0.0.1_d470b56.tgz";
  publishSha256 = "a8b12b87dac7d20bafa462e7c1bb1da355724d6a22a7adaf4e5840a62c5374cb";

  dotnet-sdk_11 = pkgs.dotnetCorePackages.sdk_11_0;

  # 主程序运行时原生依赖（原 fhsEnv targetPkgs 清单，经 makeWrapper 注入 LD_LIBRARY_PATH）。
  # ⚠️ 不含 glibc：见文件头「glibc 混用问题」。glibc 由系统默认路径提供（ld.so 自动解析）。
  runtimeLibs = with pkgs; [
    zlib
    openssl
    libGL
    libICE
    libSM
    libX11
    libXcursor
    libXext
    libXi
    libXrandr
    libXrender
    libXfixes
    libXdamage
    libXcomposite
    libxkbcommon
    gtk3
    glib
    at-spi2-core
    gdk-pixbuf
    cairo
    pango
    fontconfig.lib
    lttng-ust
    icu74
    libunwind
    libuuid
    krb5
    curl
    alsa-lib
    pulseaudio
  ];

in
stdenv.mkDerivation {
  pname = "watt-toolkit";
  # NixOS 适配版版本号（与上游源码版本解耦；当前基于上游 3.1.0）
  version = "0.0.1";

  # 声明多个输出：out（主程序）/ accelerator（加速子进程）
  # 注意：不再有 ssl 输出。系统根证书由程序运行时惰性生成到 AppData，
  # 再由程序把 .cer 导出到模块声明的可写路径，供 security.pki.certificates 读取。
  outputs = [ "out" "accelerator" ];

  src = if src != null then src else pkgs.fetchurl {
    url = publishUrl;
    sha256 = if sha256 != null then sha256 else publishSha256;
  };

  dontConfigure = true;
  # 发布 tgz 解压为多个顶层条目（Icons/ modules/ script/ native/ 等），
  # 无单一包目录：跳过 findSourceRoot，installPhase 直接使用解压后的工作目录。
  sourceRoot = ".";
  dontBuild = true;
  # 关键：nix stdenv 默认 strip 会重写 ELF 并丢弃尾部 SingleFile bundle
  # （Steam++.Accelerator 是 15.5MB 单文件，strip 后只剩 59KB → bundle 损坏）。
  dontStrip = true;

  nativeBuildInputs = [ pkgs.makeWrapper pkgs.patchelf ];

  installPhase = ''
    runHook preInstall

    # ---- 主程序（out output） ----
    mkdir -p $out
    cp -r ./* $out/
    mkdir -p $out/assemblies
    mv $out/*.dll $out/assemblies/ 2>/dev/null || true
    if [ -f "$out/Steam++.dll" ]; then
      mv $out/Steam++.dll $out/assemblies/
    fi

    # SkiaSharp 2.88 native resolver 只搜 app 目录/固定路径（不认 ../native/<rid>、不走 LD_LIBRARY_PATH）：
    # 必须把发布工具移出的原生库平铺回 assemblies/，同时保留 runtimes 布局（deps.json 声明）。
    chmod -R u+w $out/assemblies
    if [ -d "./native/linux-x64" ]; then
      mkdir -p $out/assemblies/runtimes/linux-x64/native
      cp -v ./native/linux-x64/*.so $out/assemblies/
      cp -v ./native/linux-x64/*.so $out/assemblies/runtimes/linux-x64/native/
    fi

    # ---- 关键修复：out/modules/Accelerator/ 只保留插件 UI 入口及其非 Avalonia 依赖 ----
    # 插件系统在独立 ALC 中从模块目录 LoadFrom。若模块目录存在 Steam++.Accelerator.dll
    # （加速器服务本体）或 Avalonia/BD.Common 等运行时副本，会与主程序 assemblies/ 形成
    # 双实例，导致 StandardAssetLoader 资源解析错乱（avares FileNotFoundException，UI 启动崩溃）。
    # 加速器服务本体由 $accelerator 输出（完整模块）供 systemd 服务运行，主程序包不携带。
    if [ -d "$out/modules/Accelerator" ]; then
      chmod -R u+w $out/modules/Accelerator
      (cd $out/modules/Accelerator && \
        find . -maxdepth 1 -type f \
          ! -name 'BD.WTTS.Client.Plugins.Accelerator.dll' \
          ! -name 'BD.WTTS.Primitives.dll' \
          ! -name 'BD.WTTS.Primitives.Models.dll' \
          ! -name 'BD.WTTS.Primitives.Resources.dll' \
          ! -name 'BD.WTTS.MicroServices.Primitives.dll' \
          ! -name 'BD.WTTS.MicroServices.Primitives.Models.dll' \
          ! -name 'BD.WTTS.MicroServices.Primitives.Resources.dll' \
          ! -name 'BD.WTTS.Client.IPC.dll' -delete \
        && rm -rf en es it ja ko ru zh-Hant)
      echo "out/modules/Accelerator/ 精简为插件入口+依赖: $(ls $out/modules/Accelerator | wc -l) 文件"
    fi

    # 入口 wrapper：优先官方包 bundled dotnet —— 官方 3.1.0 的 runtimeconfig 请求 net10.0，
    # bundled dotnet（10.0.4）完全匹配（与官方 Steam++.sh 启动方式一致：DOTNET_ROOT 指向包内
    # dotnet，直接运行 assemblies/Steam++.dll），避免桌面端"找不到框架"（nix dotnet 版本不匹配）。
    # 分支包（无 dotnet/ 目录，runtimeconfig 请求 net11 preview）退回 nix dotnet-sdk。
    # ⚠️ runtimeLibs 不含 glibc（见文件头），LD_LIBRARY_PATH 不再污染 fork 的子进程。
    mkdir -p $out/bin
    if [ -x "$out/dotnet/dotnet" ]; then
      chmod -R u+w $out/dotnet
      chmod +x $out/dotnet/dotnet 2>/dev/null || true
      DOTNET_BIN="$out/dotnet/dotnet"
      DOTNET_ROOT="$out/dotnet"
    else
      DOTNET_BIN="${dotnet-sdk_11}/bin/dotnet"
      DOTNET_ROOT="${dotnet-sdk_11}/share/dotnet"
    fi
    makeWrapper "$DOTNET_BIN" $out/bin/watt-toolkit \
      --set DOTNET_ROOT "$DOTNET_ROOT" \
      --set DOTNET_SYSTEM_GLOBALIZATION_INVARIANT "1" \
      --set STEAMTOOLS_BUNDLED_CER "/var/lib/watt-toolkit/SteamTools.Certificate.cer" \
      --run 'export XDG_DATA_HOME="$HOME/.local/share/WattToolkit"' \
      --prefix PATH : "${dotnet-sdk_11}/bin:${pkgs.nss_latest}/bin" \
      --prefix LD_LIBRARY_PATH : "${lib.makeLibraryPath runtimeLibs}:$DOTNET_ROOT" \
      --add-flags "$out/assemblies/Steam++.dll"
    echo "Watt Toolkit 已安装到: $out/bin/watt-toolkit (dotnet: $DOTNET_BIN)"

    # ---- Accelerator（accelerator output，NixOS 目录发布） ----
    # 加速器在 NixOS 分支构建时改为目录发布（发布工具检测 /etc/NIXOS 设 SingleFile=false）：
    # SingleFile apphost 在 NixOS 加载运行时失败（宿主退出码 203），目录发布后由
    # systemd 服务用 dotnet 直接运行 Steam++.Accelerator.dll（与主程序目录发布一致）。
    # ⚠️ 官方纯净 release 若为 SingleFile 形态（无 .dll），accelerator 输出只复制可执行，
    #    需分支发布（上游合并 NixOS 补丁后的版本）才能支持 systemd 服务运行。
    mkdir -p $accelerator
    if [ -d "./modules/Accelerator" ]; then
      cp -r "./modules/Accelerator"/* $accelerator/
      chmod -R u+w $accelerator
      chmod 755 $accelerator/Steam++.Accelerator 2>/dev/null || true
      echo "已复制加速器目录（目录发布）:"
      ls $accelerator | head -25
    else
      echo "警告: 未找到 modules/Accelerator 目录"
      find . -iname '*Accelerator*' 2>/dev/null || true
    fi

    runHook postInstall
  '';

  meta = {
    description = "Watt Toolkit (Steam++) - 开源跨平台多功能游戏工具箱";
    homepage = "https://steampp.net";
    license = lib.licenses.gpl3Only;
    mainProgram = "watt-toolkit";
    platforms = [ "x86_64-linux" ];
  };
}
