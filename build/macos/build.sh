#!/bin/bash
# 判断是否提供了 Build_Mode 参数
if [ -z "$1" ]; then
    Build_Mode="any" # 如果没有提供参数，则设置默认值为 "any"
else
    Build_Mode="$1"
fi

Base_Path="/Users/moxiansheng/Documents/GitHub/SteamTools"
App_Path="$Base_Path/src/BD.WTTS.Client.Avalonia.App"
Publish_Path="bin/Release/Publish/assemblies"
Plugins_Release_Path="bin/Release"
CodesignKey_Name="mossimo code"
App_Modules_Path="$App_Path/$Publish_Path/modules"
echo "编译主程序"
cd "$App_Path"
dotnet publish -c Release -p:PublishDir="$Publish_Path" -f net8.0-macos -p:CreatePackage=false -p:EnableCodeSigning=true -p:CodesignKey="$CodesignKey_Name"

rm -r "$App_Modules_Path"
mkdir -p "$App_Modules_Path"
echo "编译插件"
#复制插件
Copy_Plugins() {
    local Plugins_Folder=$1
    local Plugins_Folder_Name=$2
    local Plugins_FileName=$3
    mkdir -p "$App_Modules_Path/$Plugins_Folder_Name"
    cp -r "$Plugins_Folder/$Plugins_FileName" "$App_Modules_Path/$Plugins_Folder_Name/$Plugins_FileName"
}
echo "编译加速插件"
Plugins_Accelerator_Path="$Base_Path/src/BD.WTTS.Client.Plugins.Accelerator"
cd "$Plugins_Accelerator_Path"
dotnet publish -c Release -p:PublishDir="$Publish_Path" -f net8.0-macos
Copy_Plugins "$Plugins_Release_Path/net8.0-macos" "Accelerator" "BD.WTTS.Client.Plugins.Accelerator.dll"

Plugins_Accelerator_ReverseProxy_Path="$Base_Path/src/BD.WTTS.Client.Plugins.Accelerator.ReverseProxy"
cd "$Plugins_Accelerator_ReverseProxy_Path"
# 根据 Build_Mode 执行不同逻辑
if [ "$Build_Mode" = "any" ]; then
    dotnet publish -c Release -p:PublishDir="$Publish_Path/arm64" -f net8.0 -r osx-arm64 -p:PublishSingleFile=true --self-contained -p:EnableCodeSigning=true -p:CodesignKey="$CodesignKey_Name"
    dotnet publish -c Release -p:PublishDir="$Publish_Path/x64" -f net8.0 -r osx-x64 -p:PublishSingleFile=true --self-contained -p:EnableCodeSigning=true -p:CodesignKey="$CodesignKey_Name"
    lipo -create "$Publish_Path/arm64/Steam++.Accelerator" "$Publish_Path/x64/Steam++.Accelerator" -output "$Publish_Path/Steam++.Accelerator"
    Copy_Plugins "$Publish_Path" "Accelerator" "Steam++.Accelerator"
elif [ "$Build_Mode" = "arm64" ]; then
    dotnet publish -c Release -p:PublishDir="$Publish_Path/arm64" -f net8.0 -r osx-arm64 -p:PublishSingleFile=true --self-contained -p:EnableCodeSigning=true -p:CodesignKey="$CodesignKey_Name"
    Copy_Plugins "$Publish_Path/arm64" "Accelerator" "Steam++.Accelerator"
elif [ "$Build_Mode" = "x64" ]; then
    dotnet publish -c Release -p:PublishDir="$Publish_Path/x64" -f net8.0 -r osx-x64 -p:PublishSingleFile=true --self-contained -p:EnableCodeSigning=true -p:CodesignKey="$CodesignKey_Name"
    Copy_Plugins "$Publish_Path/x64" "Accelerator" "Steam++.Accelerator"
fi

echo "编译令牌插件"
Plugins_Authenticator_Path="$Base_Path/src/BD.WTTS.Client.Plugins.Authenticator"
cd "$Plugins_Authenticator_Path"
dotnet publish -c Release -p:PublishDir="$Publish_Path" -f net8.0-macos -p:EnableCodeSigning=true -p:CodesignKey="$CodesignKey_Name"
Copy_Plugins "$Plugins_Release_Path/net8.0-macos" "Authenticator" "BD.WTTS.Client.Plugins.Authenticator.dll"

echo "编译账户插件"
Plugins_GameAccount_Path="$Base_Path/src/BD.WTTS.Client.Plugins.GameAccount"
cd "$Plugins_GameAccount_Path"
dotnet publish -c Release -p:PublishDir="$Publish_Path" -f net8.0-macos -p:EnableCodeSigning=true -p:CodesignKey="$CodesignKey_Name"
Copy_Plugins "$Plugins_Release_Path/net8.0-macos" "GameAccount" "BD.WTTS.Client.Plugins.GameAccount.dll"

echo "编译游戏库插件"
Plugins_GameList_Path="$Base_Path/src/BD.WTTS.Client.Plugins.GameList"
cd "$Plugins_GameList_Path"
dotnet publish -c Release -p:PublishDir="$Publish_Path" -f net8.0-macos -p:EnableCodeSigning=true -p:CodesignKey="$CodesignKey_Name"
Copy_Plugins "$Plugins_Release_Path/net8.0-macos" "GameList" "BD.WTTS.Client.Plugins.GameList.dll"

echo "复制插件到程序目录"
cd "$App_Path"

# 根据 Build_Mode 执行不同逻辑
if [ "$Build_Mode" = "any" ]; then
    cp -r "$App_Modules_Path" "$Plugins_Release_Path/net8.0-macos/Steam++.app/Contents/MonoBundle"
elif [ "$Build_Mode" = "arm64" ]; then
    cp -r "$App_Modules_Path" "$Plugins_Release_Path/net8.0-macos/osx-arm64/Steam++.app/Contents/MonoBundle"
    cp -r "$Plugins_Release_Path/net8.0-macos/Steam++.app/Contents/Resources" "$Plugins_Release_Path/net8.0-macos/Steam++.app/Contents"
elif [ "$Build_Mode" = "x64" ]; then
    cp -r "$App_Modules_Path" "$Plugins_Release_Path/net8.0-macos/osx-x64/Steam++.app/Contents/MonoBundle"
    cp -r "$Plugins_Release_Path/net8.0-macos/Steam++.app/Contents/Resources" "$Plugins_Release_Path/net8.0-macos/Steam++.app/Contents"
fi

echo "代码签名重新手动签名"
App_Publish_Path="$App_Path/bin/Release/net8.0-macos/Steam++.app"
# #代码签名重新手动签名
codesign --remove-signature "$App_Publish_Path"
codesign --remove-signature "$App_Publish_Path/Contents/MacOS/Steam++"
dylibs=$(find "$App_Publish_Path/Contents/MonoBundle" -type f -name "*.dylib")
for dylib in $dylibs; do
    codesign --remove-signature "$dylib"
    codesign -f -s "$CodesignKey_Name" "$dylib"
done
codesign -f -s   "$CodesignKey_Name" "$App_Publish_Path/Contents/MacOS/Steam++"
codesign -f -s "$CodesignKey_Name" "$App_Publish_Path"
sudo xattr -rd com.apple.quarantine "$App_Publish_Path"
