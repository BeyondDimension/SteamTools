#!/bin/bash
run_path=$(dirname "$0")
base_path="$(dirname "$run_path")"
mkdir -p "$base_path"
cd "$base_path" || exit 1
appVer_path="$base_path/WattToolkit.AppVer"
exec_name="Steam++"
tar_name="WattToolkit.tgz"
tar_path="$base_path/$tar_name"
app_name="Watt Toolkit"
PROCESS_NAMES=("$exec_name" "$app_name")
export LC_ALL=en_US.UTF-8
Install_certutil() {
    # 判断发行版类型
    if command -v certutil &>/dev/null; then
        echo "certutil 工具已安装。"
    else
        echo "证书导入以及验证需要使用 certutil 工具。"
        # 判断包管理器
        if command -v apt &>/dev/null; then
            # 使用 apt (Debian/Ubuntu)
            sudo apt update
            sudo apt install -y libnss3-tools
        elif command -v dnf &>/dev/null; then
            # 使用 dnf (Fedora)
            sudo dnf install -y nss-tools
        elif command -v yum &>/dev/null; then
            # 使用 yum (CentOS/Red Hat)
            sudo yum install -y nss-tools
        elif command -v pacman &>/dev/null; then
            # 使用 pacman (Arch Linux)
            # sudo pacman -S nss
            echo "请手动安装 certutil 工具。"
            exit 1
        else
            echo "请手动安装 certutil 工具。"
            exit 1
        fi
        echo "certutil 工具已安装。"
    fi
}
certutil_Init() {
    certutil -d $HOME/.pki/nssdb -L
    ret=$?
    if [ $ret -ne 0 ]; then
        echo "即将初始化 certutil \$HOME/.pki/nssdb"
        mkdir -p $HOME/.pki
        mkdir -p $HOME/.pki/nssdb
        chmod 700 $HOME/.pki/nssdb
        certutil -d $HOME/.pki/nssdb -N --empty-password
    else
        echo "certutil nssdb 正常"
    fi
}
Install_jq() {
    # Check if jq is already installed
    if command -v jq &>/dev/null; then
        echo "jq 工具已安装。"
    else
        echo "jq 用来解析版本更新。"
        # Check the package manager
        if command -v apt &>/dev/null; then
            # Using apt (Debian/Ubuntu)
            sudo apt update
            sudo apt install -y jq
        elif command -v dnf &>/dev/null; then
            # Using dnf (Fedora)
            sudo dnf install -y jq
        elif command -v yum &>/dev/null; then
            # Using yum (CentOS/Red Hat)
            sudo yum install -y jq
        elif command -v pacman &>/dev/null; then
            # Using pacman (Arch Linux)
            sudo pacman -S jq
        else
            echo "请手动安装 jq 工具。"
            exit 1
        fi
        echo "请手动安装 jq 工具。"
    fi
}
Install_zenity() {
    # 判断发行版类型
    if command -v zenity &>/dev/null; then
        echo "zenity 工具已安装。"
    else
        echo "安装过程需要 zenity 工具。"
        # 判断包管理器
        if command -v apt &>/dev/null; then
            # 使用 apt (Debian/Ubuntu)
            sudo apt update
            sudo apt install -y zenity
        elif command -v dnf &>/dev/null; then
            # 使用 dnf (Fedora)
            sudo dnf install -y zenity
        elif command -v yum &>/dev/null; then
            # 使用 yum (CentOS/Red Hat)
            sudo yum install -y zenity
        elif command -v pacman &>/dev/null; then
            # 使用 pacman (Arch Linux)
            # sudo pacman -S zenity
            echo "请手动安装 zenity 工具。"
            exit 1
        else
            echo "请手动安装 zenity 工具。"
            exit 1
        fi
        echo "zenity 工具已安装。"
    fi
}

Show_Run() {
    local param1=$1
    # 显示提示框，询问是否运行程序
    zenity --question --text="$1" --width=400

    # 获取上一个命令的退出码
    response=$?

    if [ $response -eq 0 ]; then
        echo "程序已启动。"
        exit 0 &
        # 用户点击了 "运行" 按钮，启动程序
        /bin/sh -c "$base_path/$exec_name.sh"
    else
        # 用户点击了 "关闭" 按钮，退出脚本
        exit 0
    fi
}
Kill_Process() {
    # 尝试的次数
    Kill_MAX_RETRIES=3

    # 循环尝试终止进程
    for process_name in "${PROCESS_NAMES[@]}"; do
        kill_retry=1
        while [ $kill_retry -le $Kill_MAX_RETRIES ]; do
            pid=$(pgrep "$process_name")
            if [ -n "$pid" ]; then
                echo "尝试 $kill_retry: 进程 $process_name 正在运行中。正在终止..."
                kill $pid
                sleep 2
            else
                break
            fi
            kill_retry=$((kill_retry + 1))
        done
    done

    for process_name in "${PROCESS_NAMES[@]}"; do
        # 检查是否成功终止进程
        if pgrep -x "$process_name" >/dev/null; then
            echo "无法终止程序 $process_name。尝试次数已达上限。"
            exit 1
        else
            echo "程序 $process_name 已成功终止。"
        fi
    done

}
Install_certutil
Install_zenity
Install_jq
certutil_Init
Kill_Process

dotnet_exec="$dotnet_path/dotnet/dotnet"
if [ -x "$dotnet_exec" ]; then
    echo "文件具有执行权限。"
else
    chmod +x "$dotnet_exec"
fi
chmod +x "$base_path/$exec_name.sh"
# xdg-icon-resource install "$base_path/Icons/Watt-Toolkit.png" --size 128 Watt-Toolkit
InitDesktop() {
    while true; do
        # 使用 zenity 提示用户选择安装路径或使用默认路径
        choice=$(zenity --list --radiolist --title="请选择要添加到的位置" --column="选择" --column="路径" TRUE "$HOME/.local/share/applications/" FALSE "$HOME/Desktop")

        # 检查用户输入
        if [ "$choice" == "$HOME/.local/share/applications/" ]; then
            target_dir="$HOME/.local/share/applications/"
            break
        elif [ "$choice" == "$HOME/Desktop" ]; then
            target_dir="$HOME/Desktop/"
            break
        else
            echo "无效选项，请输入 1 或 2。"
        fi
    done
    #添加桌面文件
    rm -rf "$target_dir/Watt Toolkit.desktop" 2>/dev/null
    echo "#!/usr/bin/env xdg-open
[Desktop Entry]
Name=Watt Toolkit
Exec=$base_path/$exec_name.sh
Icon=$base_path/Icons/Watt-Toolkit.png
Terminal=false
Type=Application
StartupNotify=false" >"$target_dir/Watt Toolkit.desktop"
    chmod +x "$target_dir/Watt Toolkit.desktop"

}
InitDesktop
# update-desktop-database ~/.local/share/applications
#运行程序
Show_Run "下载安装完成，是否启动程序？"
exit 0
