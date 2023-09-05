#!/bin/bash
run_path=$(dirname "$0")
dotnet_path="$run_path/dotnet"
dotnet_exec="$dotnet_path/dotnet"
export DOTNET_ROOT="$dotnet_path"
link_exec="$run_path/Steam++"
# 判断符号链接是否存在
if [ -L "$link_exec" ]; then
    echo "符号链接 $link_exec 已存在"
else
    rm -rf "$link_exec"  2>/dev/null
    echo "创建符号链接 $dotnet_exec 到 $link_exec"
    ln -s "$dotnet_exec" "$link_exec"
    chmod +x "$link_exec"
fi
"$link_exec" "$run_path/assemblies/Steam++.dll" "$@"
exit
