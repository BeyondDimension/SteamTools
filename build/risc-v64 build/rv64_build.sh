#!/bin/bash
set -e

# ========== 覆盖中央包管理：显式添加指定版本的原生库 ==========
echo "Adding explicit native package versions (overriding Central Package Management)..."

dotnet add src/BD.WTTS.Client.Avalonia.App/BD.WTTS.Client.Avalonia.App.csproj \
    package SkiaSharp.NativeAssets.Linux --version 3.119.4

dotnet add src/BD.WTTS.Client.Avalonia.App/BD.WTTS.Client.Avalonia.App.csproj \
    package HarfBuzzSharp.NativeAssets.Linux --version 8.3.1.5

dotnet add src/BD.WTTS.Client.Avalonia.App/BD.WTTS.Client.Avalonia.App.csproj \
    package SQLitePCLRaw.lib.e_sqlite3 --version 2.1.11

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
    -r linux-riscv64 \
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
    -r linux-riscv64 \
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
mkdir -p native/linux-riscv64

RUNTIME_NATIVE_DIR="../realbuild/assemblies/runtimes/linux-riscv64/native"

if [ ! -d "$RUNTIME_NATIVE_DIR" ]; then
    echo "ERROR: Native runtime directory not found: $RUNTIME_NATIVE_DIR"
    echo "This means dotnet publish did not deploy linux-riscv64 native assets."
    echo "Possible causes:"
    echo "  1. The NuGet packages lack linux-riscv64 runtime support"
    echo "  2. The RuntimeIdentifier is not correctly set"
    echo "  3. The packages were not restored properly"
    exit 1
fi

cp -a "$RUNTIME_NATIVE_DIR"/*.so native/linux-riscv64/
echo "Native libraries successfully copied from publish output."

cd ..
mkdir -p Icons
cp -a ../res/icons/app/v3/Logo_512.png Icons/Watt-Toolkit.png

mkdir -p script
cp -a "../build/linux/environment_check.sh" \
   "../build/linux/init_desktop.sh" \
   "../build/linux/ISACheck.sh" \
   "../build/linux/offline_init.sh" \
   "../build/linux/online_install.sh" \
   "../build/linux/uninstall.sh" \
   script/

cd script
dos2unix *.sh
cd ../..

cp -a "build/linux/Steam++.sh" WattToolkit/
cd WattToolkit
dos2unix *.sh
cd ..

echo "finished"
