#!/bin/bash
run_path=$(dirname "$0")
dotnet_path="$run_path/dotnet"
dotnet_exec="$dotnet_path/dotnet"
export DOTNET_ROOT="$dotnet_path"
link_exec="$run_path/Steam++"
# 判断符号链接是否存在
if [ -L "$link_exec" ]; then
    case $LANG in
    zh_CN.UTF-8)
        echo "符号链接 $link_exec 已存在"
        ;;
    en_US.UTF-8)
        echo "Symbolic link $link_exec has been existed"
        ;;
    *)
        echo "Symbolic link $link_exec has been existed"
        ;;
    esac
else
    rm -rf "$link_exec"  2>/dev/null
    case $LANG in
    zh_CN.UTF-8)
        echo "创建符号链接 $dotnet_exec 到 $link_exec"
        ;;
    en_US.UTF-8)
        echo "Create symbolic link $dotnet_exec to $link_exec"
        ;;
    *)
        echo "Create symbolic link $dotnet_exec to $link_exec"
        ;;
    esac
    ln -s "$dotnet_exec" "$link_exec"
    chmod +x "$link_exec"
fi
"$link_exec" "$run_path/assemblies/Steam++.dll" "$@"
exit
