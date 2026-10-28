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
Determine_distribution() {
    # 判断发行版类型
    # 由于Linux发行版包管理器可以混装，如Debian安装Arch Linux的pacman，此处采用/etc/os-release的形式进行一次判断。
    # 读取 /etc/os-release 文件并提取 ID 字段，转换为小写
    # $installprefix是该发行版包管理器安装软件前缀
    # $nssvar是该发行版certutil包名称
    os_id=$(grep "^ID=" /etc/os-release | cut -d'=' -f2 | tr -d '"' | tr '[:upper:]' '[:lower:]')
    # 输出 ID
    echo "OS ID: $os_id"

    case "$os_id" in
    "ubuntu" | "debian" | "kali" | "mx" | "devuan" | "pureos" | "parrot" | "trisquel" | "bunsenlabs" | "deepin" | "antix" | "uos" | "kylin" | "openkylin" | "loongnix" | "gxde" | "nfsdesktop")
        echo 默认包管理器：apt
        sudo apt update
        installprefix="sudo apt install -y"
        nssvar="libnss3-tools"
        ;;
    "fedora" | "neokylin")
        echo 默认包管理器：dnf
        installprefix="sudo dnf install -y"
        nssvar="nss-tools"
        ;;
    "centos" | "rhel" | "rocky" | "alma" | "amzn" | "alt")
        echo 默认包管理器：yum
        installprefix="sudo yum install -y"
        nssvar="nss-tools"
        ;;
    "opensuse")
        echo 默认包管理器：zypper
        sudo zypper refresh
        installprefix="sudo zypper install"
        nssvar="mozilla-nss-tools"
        ;;
    "arch" | "manjaro" | "artix" | "chakra" | "blackarch" | "frugalware")
        echo 默认包管理器：pacman
        installprefix="sudo pacman -Sy"
        nssvar="nss"
        ;;
    "mageia" | "pclinuxos" | "openmandriva" | "rosa" | "vectorlinux")
        echo 默认包管理器：urpmi
        sudo urpmi.update -a
        installprefix="sudo urpmi"
        nssvar="nss-tools"
        ;;
    "slackware" | "salix" | "porteus" | "slacko")
        echo 默认包管理器：slackpkg
        sudo slackpkg update gpg
        sudo slackpkg update
        installprefix="sudo slackpkg install"
        nssvar="nss"
        ;;
    "aosc")
        echo 默认包管理器：oma
        installprefix="sudo oma install -y"
        nssvar="nss"
        ;;
    "gentoo")
        echo 默认包管理器：emerge
        sudo emerge --sync
        installprefix="sudo emerge -av"
        nssvar="nss"
        ;;
    "solus")
        echo 默认包管理器：eopkg
        sudo eopkg update-repo
        installprefix="sudo eopkg install"
        nssvar="nss-tools"
        ;;
    "clearlinux" | "nixos" | "void" | "puppy" | "tinycore" | "yongbao")
        # 冷门发行版，手动安装判断变量
        manualins="1"
        ;;
    *)
        echo 未知发行版
        manualins="1"
        ;;
    esac
}
Determine_distribution
Install_wget() {
    if command -v wget &>/dev/null; then
        echo "wget 工具已安装。"
    elif [ "$manualins" == "1" ]; then
        echo "请手动安装 wget 工具。"
    else
        echo "安装包网上下载需要使用 wget 工具。"
        # Gentoo特殊情况与一般情况
        if [ "$os_id" == "gentoo" ]; then
            $installprefix net-misc/wget
        else
            $installprefix wget
        fi
        echo "wget 工具已安装。"
    fi
}
Install_certutil() {
    if command -v certutil &>/dev/null; then
        echo "certutil 工具已安装。"
    elif [ "$manualins" == "1" ]; then
        echo "请手动安装 certutil 工具。"
    else
        echo "证书导入以及验证需要使用 certutil 工具。"
        $installprefix $nssvar
        # Loongnix 25特殊情况
        if [ "$os_id" == "loongnix" ]; then
            sudo ln -s /usr/sbin/setcap /usr/bin/setcap
            sudo apt update
            # sudo apt dist-upgrade
        else
            echo "certutil 工具已安装。"
        fi
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
    elif [ "$manualins" == "1" ]; then
        echo "请手动安装 jq 工具。"
    else
        echo "jq 用来解析版本更新。"
        $installprefix jq
        echo "jq 工具已安装。"
    fi
}
Install_zenity() {
    if command -v zenity &>/dev/null; then
        echo "zenity 工具已安装。"
    elif [ "$manualins" == "1" ]; then
        echo "请手动安装 zenity 工具。"
    else
        echo "安装过程需要 zenity 工具。"
        $installprefix zenity
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
Install_wget
Install_certutil
[ "$os_id" != "yongbao" ] && Install_zenity || echo 勇豹没有包管理器，不能安装zenity，此处以whiptail代替
Install_jq
certutil_Init
Kill_Process
dotnet_path="$base_path/dotnet"
dotnet_exec="$dotnet_path/dotnet/dotnet"
if [ -x "$dotnet_exec" ]; then
    echo "文件具有执行权限。"
else
    chmod +x "$dotnet_exec"
fi
chmod +x "$base_path/$exec_name.sh"
# xdg-icon-resource install "$base_path/Icons/Watt-Toolkit.png" --size 128 Watt-Toolkit
InitDesktop() {
    # 检查XDG_DESKTOP_DIR环境变量，如果未设置则使用默认值，支持KDE的中文桌面路径
    if command -v xdg-user-dir &>/dev/null; then
        XDG_DESKTOP_DIR=$(xdg-user-dir DESKTOP)
    else
        XDG_DESKTOP_DIR="$HOME/Desktop"
    fi

    while true; do
        # 使用 zenity 提示用户选择安装路径或使用默认路径
        choice=$(zenity --list --radiolist --title="请选择要添加到的位置" --column="选择" --column="路径" TRUE "$HOME/.local/share/applications/" FALSE "$XDG_DESKTOP_DIR")

        # 检查用户输入
        if [ "$choice" == "$HOME/.local/share/applications/" ]; then
            target_dir="$HOME/.local/share/applications/"
            break
        elif [ "$choice" == "$XDG_DESKTOP_DIR" ]; then
            target_dir="$XDG_DESKTOP_DIR"
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
