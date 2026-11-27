#!/bin/bash
#添加桌面文件
run_path=$(cd -- "$(dirname -- "$0")" && pwd) || exit 1
base_path="$(dirname -- "$run_path")"
exec_name="Steam++"
desktop_dir=$(xdg-user-dir DESKTOP 2>/dev/null) || desktop_dir=""
[ -n "$desktop_dir" ] || desktop_dir="$HOME/Desktop"
mkdir -p "$desktop_dir" || exit 1
rm -rf "$desktop_dir/Watt Toolkit.desktop" 2>/dev/null
echo "#!/usr/bin/env xdg-open
[Desktop Entry]
Name=Watt Toolkit
Exec=$base_path/$exec_name.sh
Icon=$base_path/Icons/Watt-Toolkit.png
Terminal=false
Type=Application
StartupNotify=false" >"$desktop_dir/Watt Toolkit.desktop"
chmod +x "$desktop_dir/Watt Toolkit.desktop"
exit 0
