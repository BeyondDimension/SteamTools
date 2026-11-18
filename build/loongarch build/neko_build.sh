#!/bin/bash
# =============================================================================
#  Watt Toolkit —— 龙架构 (linux-loongarch64) 构建脚本
#  用法：./neko_build.sh
#
#  ── NuGet 源 ─────────────────────────────────────────────────────────────────
#    · nuget.org —— 所有主线化包。Avalonia 12.1.x 依赖的 SkiaSharp(>= 3.119.4) /
#      HarfBuzzSharp(>= 8.3.1.3)，官方包从这些版本起就自带 linux-loongarch64 原生库，
#      而且全都已在 nuget.org 正式发布，不需要 Avalonia 的每夜版源。
#    · lnuget.loongnix.cn —— 只在 SQLITE_MODE=bundled 时兜底取 loongarch64 版 SQLite
#      （默认 SQLITE_MODE=auto 直接用系统 libsqlite3，根本不用它）
#
#  旧脚本用 dotnet add package ... --source lnuget 来“逼” NuGet 去龙芯源取包。
#  但 dotnet add package 会顺带做一次隐式还原，而 --source 会把整张源列表
#  替换掉，于是一次还原里所有包都只问 lnuget —— 这就是“为什么全在 lnuget 上爬”。
#  以前能过只是因为其它包早已躺在本地缓存里；上游升到 Avalonia 12 / net11 后，
#  一堆新版本包本地没有，就整片 NU1101/NU1102 了。
#  另外 lnuget 对没收录的包返回 500（不是 404），把它塞进普通还原会直接 NU1301，
#  所以本脚本用 RestoreSources 把普通还原限定在 nuget.org 一家。
# =============================================================================
set -euo pipefail

# ---------------------------------------------------------------------------
# 0. 常量
# ---------------------------------------------------------------------------
# 定位仓库根目录：脚本放在仓库根、或者 build/xxxx/ 子目录里都能跑
REPO_ROOT="$(cd "$(dirname "$(readlink -f "$0")")" && pwd)"
while [ ! -f "$REPO_ROOT/WattToolkit.slnx" ] && [ "$REPO_ROOT" != "/" ]; do
    REPO_ROOT="$(dirname "$REPO_ROOT")"
done
[ -f "$REPO_ROOT/WattToolkit.slnx" ] || { echo "ERROR: 找不到 WattToolkit.slnx，请在仓库内运行" >&2; exit 1; }
cd "$REPO_ROOT"

APP_PROJ="src/BD.WTTS.Client.Avalonia.App/BD.WTTS.Client.Avalonia.App.csproj"
PROXY_PROJ="src/BD.WTTS.Client.Plugins.Accelerator.ReverseProxy/BD.WTTS.Client.Plugins.Accelerator.ReverseProxy.csproj"

TFM="net11.0"
RID="linux-loongarch64"

NUGET_ORG="https://api.nuget.org/v3/index.json"
LNUGET="http://lnuget.loongnix.cn/v3/index.json"

# 普通还原只用 nuget.org。要追加自己的源（私有源、preview 源……）：
#   EXTRA_NUGET_SOURCES="https://...;https://..." ./neko_build.sh
RESTORE_SOURCES="$NUGET_ORG"
if [ -n "${EXTRA_NUGET_SOURCES:-}" ]; then
    RESTORE_SOURCES="$RESTORE_SOURCES;$EXTRA_NUGET_SOURCES"
fi

# 关掉 Avalonia 构建期遥测（Avalonia.BuildServices 会在每个项目里 exec 一个
# Collector 进程上报统计）。它对本机构建毫无用处，而且这个进程在 /run/user/<uid>
# 不可写（只读挂载、容器、某些发行版）时会因建不出命名 Mutex 直接 SIGABRT，
# 系统的崩溃报告器就会连弹好几个“服务崩溃”。UsedAvaloniaProducts 只被这个遥测
# 目标使用（见 Avalonia 包内 build/Avalonia.BuildServices.targets），置空即关闭。
NO_TELEMETRY=(-p:UsedAvaloniaProducts=)

# 可选：指定 NuGet 配置文件（例如带 fallbackPackageFolders 的只读缓存）
RESTORE_MSBUILD=(-p:RestoreSources="$RESTORE_SOURCES")
if [ -n "${WTTS_NUGET_CONFIG:-}" ]; then
    RESTORE_MSBUILD+=(-p:RestoreConfigFile="$WTTS_NUGET_CONFIG")
fi

# 运行时补丁版本：必须和 SDK 自带、也就是最后要打包进 WattToolkit/dotnet 的那份
# shared 运行时对齐。不显式给的话，带 -r 的 restore 会去 NuGet 上找
# Microsoft.NETCore.App.Runtime.<rid> 的最新补丁包——而微软根本不发布
# linux-loongarch64 的 runtime pack（只在龙芯 SDK 的 packs/ 里），于是整片 NU1101。
# 症状：ref/ 里十几个项目一起报「找不到 Microsoft.NETCore.App.Runtime.linux-loongarch64」。
DOTNET_BIN="$(command -v dotnet || true)"
RUNTIME_VERSION=""
if [ -n "$DOTNET_BIN" ]; then
    DOTNET_ROOT_DIR="$(dirname "$(readlink -f "$DOTNET_BIN")")"
    RUNTIME_VERSION="$(ls -1 "$DOTNET_ROOT_DIR/packs/Microsoft.NETCore.App.Runtime.$RID" 2>/dev/null | sort -V | tail -1 || true)"
    if [ -n "$RUNTIME_VERSION" ]; then
        RESTORE_MSBUILD+=(-p:RuntimeFrameworkVersion="$RUNTIME_VERSION")
    fi
fi

# SQLite：nuget.org 上 SQLitePCLRaw.lib.e_sqlite3 2.1.11 里没有 linux-loongarch64
# 资源（只到 riscv64），上游 sqlite-net-pcl 1.11.285 新依赖的 SourceGear.sqlite3 3.53.3
# 也没有。默认（SQLITE_MODE=auto/system）改成软链系统 libsqlite3，只有 bundled 模式
# 才用到下面这两个常量（lnuget 重打包的那份 2.1.11）。
SQLITE_ID="sqlitepclraw.lib.e_sqlite3"
SQLITE_VER="2.1.11"

# 打出来的包里必须存在的原生库
NATIVE_LIBS=(libSkiaSharp.so libHarfBuzzSharp.so libe_sqlite3.so)

# ---------------------------------------------------------------------------
# 小工具
# ---------------------------------------------------------------------------
die() { echo "ERROR: $*" >&2; exit 1; }

# ---------------------------------------------------------------------------
# 日志：dotnet 的真实输出同时打到终端和 build/logs/build-<时间>.log
# ---------------------------------------------------------------------------
# MSBuild 的详细程度（quiet/minimal/normal/detailed 或 q/m/n/d）：
#   默认 m —— 会打出每个项目的真实产物行，例如
#     BD.WTTS.Client -> /home/.../bin/Release/net11.0/BD.WTTS.Client.dll
#   想看 MSBuild 目标级细节：BUILD_VERBOSITY=n ./neko_build.sh
BUILD_VERBOSITY="${BUILD_VERBOSITY:-m}"
BUILD_LOG_DIR="build/logs"
mkdir -p "$BUILD_LOG_DIR"
BUILD_LOG="${BUILD_LOG:-$BUILD_LOG_DIR/build-$(date +%Y%m%d-%H%M%S).log}"

say() { echo "$*" | tee -a "$BUILD_LOG"; }

# run <命令...>：回显命令行 → 跑 → 输出同时进终端和日志，退出码原样透传
run() {
    say "  \$ $*"
    local st=0
    "$@" 2>&1 | tee -a "$BUILD_LOG" || st=$?
    return "$st"
}

# ELF 的 e_machine（小端）：LoongArch=0201，x86_64=3e00，aarch64=b700
elf_machine() { od -An -tx1 -j18 -N2 "$1" | tr -d ' \n'; }

sha512_b64() {
    if command -v openssl >/dev/null 2>&1; then
        openssl dgst -sha512 -binary "$1" | base64 | tr -d '\n'
    else
        sha512sum "$1" | cut -d' ' -f1 | xxd -r -p | base64 | tr -d '\n'
    fi
}

# ---------------------------------------------------------------------------
# 1. 环境检查
# ---------------------------------------------------------------------------
say "==> [1/6] 检查环境"
command -v dotnet >/dev/null 2>&1 || die "找不到 dotnet，请先安装 .NET 11 SDK"
command -v curl   >/dev/null 2>&1 || die "找不到 curl"
command -v unzip  >/dev/null 2>&1 || die "找不到 unzip"
[ "$(uname -m)" = "loongarch64" ] || say "    提示：当前架构是 $(uname -m)，不是 loongarch64"
say "    dotnet $(dotnet --version)"

# 关掉 NuGet 包签名校验：SDK 自带的 codesignctl.pem 回退信任链在部分龙芯发行版上
# 会把正常包判成 NU3012“证书已被吊销”（Splat 19.3.1 就是典型），直接卡死 restore。
export DOTNET_NUGET_SIGNATURE_VERIFICATION="${DOTNET_NUGET_SIGNATURE_VERIFICATION:-false}"

# ---------------------------------------------------------------------------
# 2. 决定 SQLite 原生库怎么来
# ---------------------------------------------------------------------------
# 为什么会缺：nuget.org 上 SQLitePCLRaw.lib.e_sqlite3 2.1.11 里没有
# linux-loongarch64 资源（只有 riscv64）；上游 sqlite-net-pcl 1.11.285 新依赖的
# SourceGear.sqlite3 3.53.3 也没有（同样只到 riscv64）。
#
# 做法：libe_sqlite3.so 和系统 libsqlite3 导出的是同一套 sqlite3_* C API
# （e_sqlite3 多出来的只是 SEE 之类付费功能，本项目用不到），而 AOSC / Deepin /
# Loongnix 这些龙架构发行版都自带最新版 SQLite。所以直接把 libe_sqlite3.so 做成
# 指向系统 libsqlite3.so.0 的软链即可，顺带还能用上发行版最新的 SQLite（3.53.x）。
#
#   SQLITE_MODE=auto     找得到系统库就软链，找不到退回 bundled（默认）
#   SQLITE_MODE=system   强制软链系统库，找不到直接报错
#   SQLITE_MODE=bundled  把 lnuget 重打包的 libe_sqlite3.so 塞进包里
#                        （自带、跨机器可迁移，但 SQLite 版本老，且需要 lnuget 可达）
SQLITE_MODE="${SQLITE_MODE:-auto}"
SQLITE_SYSTEM_LIB=""

find_system_sqlite() {
    local cand name
    # 优先运行库 soname（不需要 -dev 包），再退到开发用软链
    if command -v ldconfig >/dev/null 2>&1; then
        for name in libsqlite3.so.0 libsqlite3.so; do
            cand="$(ldconfig -p 2>/dev/null | awk -v n="$name" '$1 == n { print $NF; exit }')"
            if [ -n "$cand" ] && [ -e "$cand" ]; then
                echo "$cand"
                return 0
            fi
        done
    fi
    for cand in /usr/lib/libsqlite3.so.0 /lib/libsqlite3.so.0 \
                /usr/lib64/libsqlite3.so.0 /lib64/libsqlite3.so.0 \
                /usr/lib/libsqlite3.so /lib/libsqlite3.so; do
        if [ -e "$cand" ]; then
            echo "$cand"
            return 0
        fi
    done
    return 1
}

case "$SQLITE_MODE" in
    bundled) ;;
    system)  SQLITE_SYSTEM_LIB="$(find_system_sqlite)" || die "SQLITE_MODE=system，但找不到系统 libsqlite3" ;;
    auto)    SQLITE_SYSTEM_LIB="$(find_system_sqlite || true)"; [ -n "$SQLITE_SYSTEM_LIB" ] || SQLITE_MODE=bundled ;;
    *)       die "SQLITE_MODE 只能是 auto / system / bundled" ;;
esac

# 把 libe_sqlite3.so 换/补成指向系统库的软链（bundled 模式下由 NuGet 包自己落盘，什么都不做）
install_sqlite_native() {
    local dir=$1
    [ -n "$SQLITE_SYSTEM_LIB" ] || return 0
    ln -sfn "$SQLITE_SYSTEM_LIB" "$dir/libe_sqlite3.so"
}

if [ -n "$SQLITE_SYSTEM_LIB" ]; then
    [ "$(elf_machine "$SQLITE_SYSTEM_LIB")" = "0201" ] \
        || die "$SQLITE_SYSTEM_LIB 不是 LoongArch 目标文件"
    say "==> [2/6] SQLite：软链到系统库 $SQLITE_SYSTEM_LIB"
else
    say "==> [2/6] SQLite：打包 lnuget 的 libe_sqlite3.so（bundled 模式）"
    # 注意：NuGet 只要 id+版本 在全局包目录里命中（<NUGET_PACKAGES>/<id>/<ver>），就直接用
    # 里面已解包的内容，根本不会再问远端源。所以光“换源”没用 —— 旧缓存里那份
    # nuget.org 版本（不带 loongarch64）会一直胜出，必须就地覆盖。
    #
    # lnuget 经常抽风（广东电信对 lnuget 进行了屏蔽，得用数据连），所以 .nupkg 会在本地留一份：
    #   build/lnuget-cache/sqlitepclraw.lib.e_sqlite3.2.1.11.nupkg
    # 只要它在，bundled 模式就完全不需要联 lnuget（build/ 已被 .gitignore 忽略）。
    NUGET_ROOT="${NUGET_PACKAGES:-$HOME/.nuget/packages}"
    SQLITE_DIR="$NUGET_ROOT/$SQLITE_ID/$SQLITE_VER"
    SQLITE_SO_REL="runtimes/$RID/native/libe_sqlite3.so"
    SQLITE_NUPKG_CACHE="build/lnuget-cache/$SQLITE_ID.$SQLITE_VER.nupkg"

    if [ -f "$SQLITE_DIR/$SQLITE_SO_REL" ]; then
        say "    已就绪：$SQLITE_DIR/$SQLITE_SO_REL"
    else
        mkdir -p "$(dirname "$SQLITE_NUPKG_CACHE")"
        if [ -f "$SQLITE_NUPKG_CACHE" ]; then
            say "    使用本地缓存：$SQLITE_NUPKG_CACHE"
            nupkg="$SQLITE_NUPKG_CACHE"
        else
            say "    本地无缓存，从 lnuget 下载……"
            tmp_dir="$(mktemp -d)"
            trap 'rm -rf "$tmp_dir"' EXIT
            if ! curl -fSL --retry 5 --retry-delay 2 --connect-timeout 15 --max-time 300 \
                    -o "$tmp_dir/pkg.nupkg" \
                    "$LNUGET/package/$SQLITE_ID/$SQLITE_VER/$SQLITE_ID.$SQLITE_VER.nupkg"; then
                die "从 lnuget 下载 $SQLITE_ID $SQLITE_VER 失败。
        lnuget 时不时会挂，可以离线处理：把下面的文件
          $LNUGET/package/$SQLITE_ID/$SQLITE_VER/$SQLITE_ID.$SQLITE_VER.nupkg
        手动下载后放到
          $(pwd)/$SQLITE_NUPKG_CACHE
        再重新运行本脚本即可；或者干脆改用系统 SQLite：
          SQLITE_MODE=system ./neko_build.sh"
            fi
            mv "$tmp_dir/pkg.nupkg" "$SQLITE_NUPKG_CACHE"
            rm -rf "$tmp_dir"
            trap - EXIT
            nupkg="$SQLITE_NUPKG_CACHE"
        fi
        rm -rf "$SQLITE_DIR"
        mkdir -p "$SQLITE_DIR"
        cp "$nupkg" "$SQLITE_DIR/$SQLITE_ID.$SQLITE_VER.nupkg"
        unzip -q -o "$SQLITE_DIR/$SQLITE_ID.$SQLITE_VER.nupkg" -d "$SQLITE_DIR"
        # lnuget 这份 zip 里文件权限是 000，不改回来 publish 复制/运行都会失败
        chmod -R u+rwX,go+rX "$SQLITE_DIR"
        hash="$(sha512_b64 "$SQLITE_DIR/$SQLITE_ID.$SQLITE_VER.nupkg")"
        printf '%s' "$hash" > "$SQLITE_DIR/$SQLITE_ID.$SQLITE_VER.nupkg.sha512"
        printf '{\n  "version": 2,\n  "contentHash": "%s",\n  "source": "%s"\n}\n' \
            "$hash" "$LNUGET" > "$SQLITE_DIR/.nupkg.metadata"
        [ -f "$SQLITE_DIR/$SQLITE_SO_REL" ] || die "lnuget 的 $SQLITE_ID 里没有 $SQLITE_SO_REL"
        say "    已解包：$SQLITE_DIR/$SQLITE_SO_REL"
    fi

    [ "$(elf_machine "$SQLITE_DIR/$SQLITE_SO_REL")" = "0201" ] \
        || die "$SQLITE_DIR/$SQLITE_SO_REL 不是 LoongArch 目标文件"
fi

# ---------------------------------------------------------------------------
# 3. 还原
# ---------------------------------------------------------------------------
say "==> [3/6] 还原主程序与反代插件"
# 反代插件一会儿要用 UseAppHost=true（单文件 exe）。注意：不能在 restore 阶段
# 带上 UseAppHost=true —— 那会让 ref/dotnetCampus.Ipc 的 netcoreapp3.1 目标也去找
# linux-loongarch64 的 apphost 包（NETSDK1084：这个 TFM 的龙芯 apphost 根本不存在）。
# 所以 restore 用默认值，publish 时再用 --no-restore + UseAppHost=true。
run dotnet restore "$APP_PROJ"   -r "$RID" -v "$BUILD_VERBOSITY" "${RESTORE_MSBUILD[@]}"
run dotnet restore "$PROXY_PROJ" -r "$RID" -v "$BUILD_VERBOSITY" "${RESTORE_MSBUILD[@]}"

# ---------------------------------------------------------------------------
# 4. 发布
# ---------------------------------------------------------------------------
BASE_BUILD_ARGS=(
    -c Release
    -p:PublishReadyToRun=false
    -p:PublishDocumentationFile=false
    -p:PublishDocumentationFiles=false
    -p:PublishReferencesDocumentationFiles=false
    -f "$TFM"
    -r "$RID"
    -v "$BUILD_VERBOSITY"
    /property:WarningLevel=1
    --sc false
    --nologo
)
# 与 restore 保持同一个运行时版本，避免 publish 阶段又去追别的补丁包
if [ -n "$RUNTIME_VERSION" ]; then
    BASE_BUILD_ARGS+=(-p:RuntimeFrameworkVersion="$RUNTIME_VERSION")
fi

rm -rf realbuild/assemblies realbuild/modules

say "==> [4/6] 发布主程序"
run dotnet publish "$APP_PROJ" \
    -p:UseAppHost=false \
    -p:PublishSingleFile=false \
    -p:PublishDir=realbuild/assemblies \
    "${BASE_BUILD_ARGS[@]}" \
    "${NO_TELEMETRY[@]}" \
    --no-restore \
    -o "realbuild/assemblies"
install_sqlite_native "realbuild/assemblies"

say "==> [4/6] 发布反代插件（单文件 exe）"
run dotnet publish "$PROXY_PROJ" \
    -p:UseAppHost=true \
    -p:PublishSingleFile=true \
    -p:PublishDir=realbuild/modules/Accelerator \
    "${BASE_BUILD_ARGS[@]}" \
    "${NO_TELEMETRY[@]}" \
    --no-restore \
    -o "realbuild/modules/Accelerator"
# 单文件 exe 旁边的原生库（未打进 bundle，运行时按 exe 所在目录找）
install_sqlite_native "realbuild/modules/Accelerator"

say "==> [4/6] 编译其余插件"
# 插件列表要和 PluginsCore 的模块名一致：modules/<名字>/BD.WTTS.Client.Plugins.<名字>.dll
# 注意 v3.3.0 起 ArchiSteamFarmPlus / GameTools 也走同一套模块加载（AOT 构建里是静态链接的），
# JIT 构建必须自己把它们编成模块，否则侧边栏会少「ASF 控制台」「游戏工具」两页。
for plugin in GameAccount GameList Authenticator SteamIdleCard Accelerator GameTools ArchiSteamFarmPlus; do
    say "    - $plugin"
    run dotnet build -c Release "src/BD.WTTS.Client.Plugins.$plugin/BD.WTTS.Client.Plugins.$plugin.csproj" \
        --nologo -v "$BUILD_VERBOSITY" --property:WarningLevel=1 --property:DebugType=pdbonly \
        -o "realbuild/modules/$plugin" \
        "${RESTORE_MSBUILD[@]}" \
        "${NO_TELEMETRY[@]}"
done

# ---------------------------------------------------------------------------
# 5. 校验原生库
# ---------------------------------------------------------------------------
# 注意：--sc false + -r linux-loongarch64 发布时，.NET 11 会把 runtimes/<rid>/native/*.so
# 直接摊平到输出根目录（旧 SDK 会保留 runtimes/ 子树），两种布局都要认。
say "==> [5/6] 校验 linux-loongarch64 原生库"
PUBLISH_DIR="realbuild/assemblies"
NATIVE_DIRS=()
[ -d "$PUBLISH_DIR/runtimes/$RID/native" ] && NATIVE_DIRS+=("$PUBLISH_DIR/runtimes/$RID/native")
NATIVE_DIRS+=("$PUBLISH_DIR")

find_native() {
    local name=$1 dir
    for dir in "${NATIVE_DIRS[@]}"; do
        if [ -f "$dir/$name" ]; then
            echo "$dir/$name"
            return 0
        fi
    done
    return 1
}

for lib in "${NATIVE_LIBS[@]}"; do
    path="$(find_native "$lib")" || die "发布结果里找不到 $lib（缺 loongarch64 原生资源）"
    machine="$(elf_machine "$path")"
    [ "$machine" = "0201" ] || die "$path 不是 LoongArch 目标文件 (e_machine=$machine)"
    if [ -L "$path" ]; then
        say "    ✓ $lib -> $(readlink "$path")"   # system 模式：指向发行版自带的新版 SQLite
    else
        say "    ✓ $lib ($(du -h "$path" | cut -f1)) <- $path"
    fi
done

# ---------------------------------------------------------------------------
# 6. 组装发行目录
# ---------------------------------------------------------------------------
say "==> [6/6] 组装 WattToolkit/"
rm -rf WattToolkit
mkdir -p WattToolkit
cp -a realbuild/assemblies WattToolkit/

mkdir -p WattToolkit/modules/{Accelerator,Authenticator,GameAccount,GameList,SteamIdleCard,GameTools,ArchiSteamFarmPlus}
cp -a "realbuild/modules/Accelerator/BD.WTTS.Client.Plugins.Accelerator.dll" \
      "realbuild/modules/Accelerator/Steam++.Accelerator" WattToolkit/modules/Accelerator/
# 单文件 exe 旁边的原生库（未打进 bundle，运行时按 exe 所在目录找）
cp -a realbuild/modules/Accelerator/*.so WattToolkit/modules/Accelerator/ 2>/dev/null || true
cp -a "realbuild/modules/Authenticator/BD.WTTS.Client.Plugins.Authenticator.dll" WattToolkit/modules/Authenticator/
cp -a "realbuild/modules/GameAccount/BD.WTTS.Client.Plugins.GameAccount.dll"     WattToolkit/modules/GameAccount/
cp -a "realbuild/modules/GameList/BD.WTTS.Client.Plugins.GameList.dll"           WattToolkit/modules/GameList/
cp -a "realbuild/modules/SteamIdleCard/BD.WTTS.Client.Plugins.SteamIdleCard.dll" WattToolkit/modules/SteamIdleCard/
cp -a "realbuild/modules/GameTools/BD.WTTS.Client.Plugins.GameTools.dll"                     WattToolkit/modules/GameTools/
cp -a "realbuild/modules/ArchiSteamFarmPlus/BD.WTTS.Client.Plugins.ArchiSteamFarmPlus.dll"   WattToolkit/modules/ArchiSteamFarmPlus/

# 自带 .NET 运行时
mkdir -p WattToolkit/dotnet
dotnetloc="$(readlink -f "$(command -v dotnet)")"
cp -a "$dotnetloc" \
      "$(dirname "$dotnetloc")/host" \
      "$(dirname "$dotnetloc")/shared" \
      "$(dirname "$dotnetloc")/LICENSE.txt" \
      "$(dirname "$dotnetloc")/ThirdPartyNotices.txt" \
      WattToolkit/dotnet/

# 原生库：程序集目录和发行根目录都放一份，覆盖两种加载路径
#   · GlobalDllImportResolver -> <BaseDirectory>/native/<RID>/lib*.so
#   · .NET 默认探测          -> <BaseDirectory>/lib*.so（摊平后的位置）
mkdir -p "WattToolkit/assemblies/native/$RID" "WattToolkit/native/$RID"
for lib in "${NATIVE_LIBS[@]}"; do
    src="$(find_native "$lib")"
    cp -a "$src" "WattToolkit/assemblies/native/$RID/$lib"
    cp -a "$src" "WattToolkit/native/$RID/$lib"
done
say "    原生库已放入 assemblies/native/$RID 与 native/$RID"

mkdir -p WattToolkit/Icons
cp -a "src/BD.WTTS.Client.Avalonia.App/Assets.xcassets/AppIcon.appiconset/Icon128.png" \
      WattToolkit/Icons/Watt-Toolkit.png

mkdir -p WattToolkit/script
cp -a build/linux/{environment_check.sh,init_desktop.sh,ISACheck.sh,Linux.sh,offline_init.sh,online_install.sh,uninstall.sh} \
      WattToolkit/script/
cp -a build/linux/Steam++.sh WattToolkit/Steam++.sh
chmod +x WattToolkit/Steam++.sh WattToolkit/script/*.sh

say ""
say "finished: $(pwd)/WattToolkit"
say "完整构建日志：$(pwd)/$BUILD_LOG"
