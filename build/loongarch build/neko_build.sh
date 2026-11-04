#!/bin/bash
set -e

# ========== 添加龙芯 NuGet 源（如尚未添加）==========
echo "Configuring NuGet sources..."
if ! dotnet nuget list source | grep -q "lnuget.loongnix.cn"; then
    dotnet nuget add source -n lnuget.loongnix.cn --protocol-version 3 \
        https://lnuget.loongnix.cn/v3/index.json \
        --allow-insecure-connections
    echo "Added loongarch64 NuGet source."
else
    echo "Loongarch64 NuGet source already configured."
fi

# ========== 覆盖中央包管理：显式添加指定版本的原生库 ==========
echo "Adding explicit native package versions (overriding Central Package Management)..."

# SkiaSharp 和 HarfBuzzSharp：官方源已有 loongarch64 支持
dotnet add src/BD.WTTS.Client.Avalonia.App/BD.WTTS.Client.Avalonia.App.csproj \
    package SkiaSharp.NativeAssets.Linux --version 3.119.4

dotnet add src/BD.WTTS.Client.Avalonia.App/BD.WTTS.Client.Avalonia.App.csproj \
    package HarfBuzzSharp.NativeAssets.Linux --version 8.3.1.5

# SQLite：必须从龙芯源获取 loongarch64 原生库
dotnet add src/BD.WTTS.Client.Avalonia.App/BD.WTTS.Client.Avalonia.App.csproj \
    package SQLitePCLRaw.lib.e_sqlite3 --version 2.1.11 \
    --source https://lnuget.loongnix.cn/v3/index.json

# 还原工作负载和包
dotnet workload restore src/BD.WTTS.Client.Avalonia.App/BD.WTTS.Client.Avalonia.App.csproj

# ========== 发布主程序 ==========
echo "Publishing main application..."
dotnet publish -c Release src/BD.WTTS.Client.Avalonia.App/BD.WTTS.Client.Avalonia.App.csproj \
    -p:UseAppHost=false \
    -p:PublishDir=realbuild/assemblies \
    -p:PublishSingleFile=false \
    -p:PublishReadyToRun=false \
    -p:PublishDocumentationFile=false \
    -p:PublishDocumentationFiles=false \
    -p:PublishReferencesDocumentationFiles=false \
    -f net10.0 \
    -r linux-loongarch64 \
    -v q \
    /property:WarningLevel=1 \
    --sc false \
    --force \
    --nologo \
    -o "realbuild/assemblies"

# ========== 发布插件 ==========
echo "Publishing plugins..."

dotnet publish -c Release src/BD.WTTS.Client.Plugins.Accelerator.ReverseProxy/BD.WTTS.Client.Plugins.Accelerator.ReverseProxy.csproj \
    -p:UseAppHost=true \
    -p:PublishDir=realbuild/modules/Accelerator \
    -p:PublishSingleFile=true \
    -p:PublishReadyToRun=false \
    -p:PublishDocumentationFile=false \
    -p:PublishDocumentationFiles=false \
    -p:PublishReferencesDocumentationFiles=false \
    -f net10.0 \
    -r linux-loongarch64 \
    -v q \
    /property:WarningLevel=1 \
    --sc false \
    --force \
    --nologo \
    -o "realbuild/modules/Accelerator"

dotnet build -c Release src/BD.WTTS.Client.Plugins.GameAccount/BD.WTTS.Client.Plugins.GameAccount.csproj \
    --nologo -v q --property:WarningLevel=1 --property:DebugType=pdbonly \
    -o "realbuild/modules/GameAccount"

dotnet build -c Release src/BD.WTTS.Client.Plugins.GameList/BD.WTTS.Client.Plugins.GameList.csproj \
    --nologo -v q --property:WarningLevel=1 --property:DebugType=pdbonly \
    -o "realbuild/modules/GameList"

dotnet build -c Release src/BD.WTTS.Client.Plugins.Authenticator/BD.WTTS.Client.Plugins.Authenticator.csproj \
    --nologo -v q --property:WarningLevel=1 --property:DebugType=pdbonly \
    -o "realbuild/modules/Authenticator"

dotnet build -c Release src/BD.WTTS.Client.Plugins.SteamIdleCard/BD.WTTS.Client.Plugins.SteamIdleCard.csproj \
    --nologo -v q --property:WarningLevel=1 --property:DebugType=pdbonly \
    -o "realbuild/modules/SteamIdleCard"

dotnet build -c Release src/BD.WTTS.Client.Plugins.Accelerator/BD.WTTS.Client.Plugins.Accelerator.csproj \
    --nologo -v q --property:WarningLevel=1 --property:DebugType=pdbonly \
    -o "realbuild/modules/Accelerator"

# dotnet test src/BD.WTTS.UnitTest/BD.WTTS.UnitTest.csproj -c Release -p:GeneratePackageOnBuild=false --nologo -v q --property:WarningLevel=1 --property:DebugType=pdbonly -a loongarch64 -o "realbuild/"

echo "Publish Done!"
echo "Start Copy File"

# ========== 组装输出目录 ==========
mkdir -p WattToolkit
cp -a realbuild/assemblies WattToolkit/

cd WattToolkit
mkdir -p modules
cd modules
mkdir -p Accelerator Authenticator GameAccount GameList SteamIdleCard

cp "../../realbuild/modules/Accelerator/BD.WTTS.Client.Plugins.Accelerator.dll" \
   "../../realbuild/modules/Accelerator/Steam++.Accelerator" Accelerator/
cp "../../realbuild/modules/Authenticator/BD.WTTS.Client.Plugins.Authenticator.dll" Authenticator/
cp "../../realbuild/modules/GameAccount/BD.WTTS.Client.Plugins.GameAccount.dll" GameAccount/
cp "../../realbuild/modules/GameList/BD.WTTS.Client.Plugins.GameList.dll" GameList/
cp "../../realbuild/modules/SteamIdleCard/BD.WTTS.Client.Plugins.SteamIdleCard.dll" SteamIdleCard/

cd ..
mkdir -p dotnet
dotnetloc=$(readlink -f $(which dotnet))
cp -a "$dotnetloc" \
   "$(dirname "$dotnetloc")/host" \
   "$(dirname "$dotnetloc")/shared" \
   "$(dirname "$dotnetloc")/LICENSE.txt" \
   "$(dirname "$dotnetloc")/ThirdPartyNotices.txt" \
   dotnet/

# ========== 严格模式：原生库必须从 publish 输出复制 ==========
mkdir -p native/linux-loongarch64

RUNTIME_NATIVE_DIR="../realbuild/assemblies/runtimes/linux-loongarch64/native"

if [ ! -d "$RUNTIME_NATIVE_DIR" ]; then
    echo "ERROR: Native runtime directory not found: $RUNTIME_NATIVE_DIR"
    echo "This means dotnet publish did not deploy linux-loongarch64 native assets."
    echo "Possible causes:"
    echo "  1. The NuGet packages lack linux-loongarch64 runtime support"
    echo "  2. The RuntimeIdentifier is not correctly set"
    echo "  3. The packages were not restored properly"
    echo "  4. SQLitePCLRaw.lib.e_sqlite3 was not fetched from lnuget.loongnix.cn"
    exit 1
fi

cp -a "$RUNTIME_NATIVE_DIR"/*.so native/linux-loongarch64/
echo "Native libraries successfully copied from publish output."

cd ..
mkdir -p Icons
cp -a "../src/BD.WTTS.Client.Avalonia.App/Assets.xcassets/AppIcon.appiconset/Icon128.png" Icons/Watt-Toolkit.png

mkdir -p script
cp -a "../build/linux/environment_check.sh" \
   "../build/linux/init_desktop.sh" \
   "../build/linux/ISACheck.sh" \
   "../build/linux/Linux.sh" \
   "../build/linux/offline_init.sh" \
   "../build/linux/online_install.sh" \
   "../build/linux/uninstall.sh" \
   script/

cp -a "../build/linux/Steam++.sh" Steam++.sh
cd ..

echo "finished"
