#!/bin/bash
#添加桌面文件
run_path=$(dirname "$0")
base_path="$(dirname "$run_path")"
exec_name="Steam++"
# 检查XDG_DESKTOP_DIR环境变量，如果未设置则使用默认值，支持KDE的中文桌面路径
if command -v xdg-user-dir &>/dev/null; then
    XDG_DESKTOP_DIR=$(xdg-user-dir DESKTOP)
else
    XDG_DESKTOP_DIR="$HOME/Desktop"
fi
rm -rf "$XDG_DESKTOP_DIR/Watt Toolkit.desktop" 2>/dev/null
echo "#!/usr/bin/env xdg-open
[Desktop Entry]
Name=Watt Toolkit
Exec=$base_path/$exec_name.sh
Icon=$base_path/Icons/Watt-Toolkit.png
Terminal=false
Type=Application
StartupNotify=false" >"$XDG_DESKTOP_DIR/Watt Toolkit.desktop"
chmod +x "$XDG_DESKTOP_DIR/Watt Toolkit.desktop"
exit 0
