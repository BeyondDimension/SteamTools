#!/bin/bash
#添加桌面文件
run_path=$(dirname "$0")
base_path="$(dirname "$run_path")"
exec_name="Steam++"
rm -rf "$HOME/Desktop/Watt Toolkit.desktop" 2>/dev/null
echo "#!/usr/bin/env xdg-open
[Desktop Entry]
Name=Watt Toolkit
Exec=$base_path/$exec_name.sh
Icon=$base_path/Icons/Watt-Toolkit.png
Terminal=false
Type=Application
StartupNotify=false" >"$HOME/Desktop/Watt Toolkit.desktop"
chmod +x "$HOME/Desktop/Watt Toolkit.desktop"
exit 0
