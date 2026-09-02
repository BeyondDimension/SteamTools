#!/bin/bash
# 设置默认的 base_path 参数
default_base_path="$HOME/WattToolkit"
base_path="$default_base_path"

# 检查是否以root身份执行
if [ "$EUID" -eq 0 ]; then
  echo "此脚本不能以root身份执行。"
  exit 1
fi

#判断系统是使用Glibc库还是musl库
command -v ldd &>/dev/null || { echo "Error: ldd tools is missing, please install ldd in your system."; exit 1; }
ldd --version 2>&1 | grep -qi 'musl' && { echo "Error: Watt Toolkit didn't support Linux distribution which use musl library, please use another tools." >&2; exit 1; } || echo "系统使用Glibc，继续运行..."

command -v dialog &>/dev/null && dialog1="dialog" || dialog1="whiptail"
# 循环直到用户输入有效路径或直接回车
while true; do
    # 使用 zenity 提示用户选择安装路径或使用默认路径（若未安装zenity，调用默认对话框工具）
    if command -v zenity &>/dev/null; then
        custom_base_path=$(zenity --entry --title="安装路径" --text="请输入安装路径（默认为 "$default_base_path"，不输入则使用默认路径）")
    else
        custom_base_path=$($dialog1 --title "安装路径" --inputbox "请输入安装路径（默认为 "$default_base_path"，不输入则使用默认路径）" 10 60 3>&1 1>&2 2>&3)
    fi
    # 点确定即使不输入也进行下一步，点取消取消安装
    case $? in
    0) ;;
    1) exit 1 ;;
    *) echo "发生意外错误" ;;
    esac
    # 如果用户提供了自定义路径，则使用该路径
    if [ -n "$custom_base_path" ]; then
        base_path="$custom_base_path"
        break
    elif [ -z "$custom_base_path" ]; then
        # 用户直接回车，使用默认路径
        break
    else
        # 用户输入无效路径，重新提示
        echo "无效路径，请重新输入。"
    fi
done

# 创建安装路径
mkdir -p "$base_path" || {
    echo "无法创建安装路径 $base_path，请检查权限或路径是否正确。"
    exit 1
}

# 切换到安装路径
cd "$base_path" || {
    echo "无法切换到安装路径 $base_path，请检查权限或路径是否正确。"
    exit 1
}

appVer_path="$base_path/WattToolkit.AppVer"
exec_name="Steam++"
tar_name="WattToolkit.tgz"
tar_path="$base_path/$tar_name"
base_url="https://api.steampp.net"
architecture=1
app_name="Watt Toolkit"
PROCESS_NAMES=("$exec_name" "$app_name")
Check_LC_Code() {
    # 检查当前 LANG 环境变量
    current_lang="${LC_ALL:-$LANG}"
    echo "当前语言环境: $current_lang"

    # 检查是否为 C 或 C.UTF-8
    if [ "$current_lang" = "C" ] || [ "$current_lang" = "C.UTF-8" ]; then
        echo "当前语言环境是 C 或 C.UTF-8，需要修改。"

        # 列出系统支持的语言环境
        supported_locales=$(locale -a)
        if [ -z "$supported_locales" ]; then
            echo "未能获取到支持的语言环境列表。请检查系统配置。"
            return 1
        fi

        # 遍历支持的语言环境列表，找到第一个不是 C 或 C.UTF-8 的项
        first_non_c_locale=""
        for locale in $supported_locales; do
            if [ "$locale" != "C" ] && [ "$locale" != "C.UTF-8" ]; then
                first_non_c_locale="$locale"
                break
            fi
        done

        # 检查是否找到了非 C/C.UTF-8 的语言环境
        if [ -n "$first_non_c_locale" ]; then
            echo "修改语言环境为支持列表中的第一个非 C/C.UTF-8 项：$first_non_c_locale"
            export LC_ALL="$first_non_c_locale"
            echo "语言环境已修改为 $first_non_c_locale。"
        else
            echo "未能找到非 C/C.UTF-8 的语言环境。请检查系统配置。"
            return 1
        fi
    else
        echo "当前语言环境不是 C 或 C.UTF-8，无需修改。"
    fi
}
Check_LC_Code
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
#精简版系统可能没有该工具
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
    if [ "$os_id" != "yongbao" ]; then zenity --question --text="$1" --width=400; else whiptail --yesno "$1" 10 60; fi

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
Get_NewVer() {
    #获取系统架构(32位不再受到本软件支持。32位处理器不可能运行勇豹yongbao系统，不做另行判断；“未知的设备架构”处适用于64位处理器与yongbao系统等Linux发行版)
    arch=$(uname -m)
    case $arch in
    x86_64)
        architecture=1
        ;;
    aarch64)
        architecture=3
        ;;
    loongarch64 | loong64)
        architecture=6
        ;;
    i?86 | arm*)
        zenity --info --text="Watt Toolkit不再支持32位，32位用户请自行在Github/Gitee下载旧版使用，谢谢。" --width=300
        exit 244
        ;;
    *)
        [ "$os_id" != "yongbao" ] && zenity --info --text="未知的设备架构:$arch!" --width=300 || whiptail --msgbox "未知的设备架构:$arch!" 10 60
        exit 244
        ;;
    esac

    # 获取发行版信息
    read -r os_version <<<"$(cat /etc/os-release | grep -E 'VERSION_ID=' | awk -F'=' '{ print $2 }' | tr -d '"')"

    # 如果 VERSION 为空，则使用 BUILD_ID 填充
    if [ -z "$os_version" ]; then
        os_version=$(cat /etc/os-release | grep -E 'BUILD_ID=' | awk -F'=' '{ print $2 }' | tr -d '"')
    fi

    # 假如是勇豹系统用下面的命令判断
    [ "$os_id" != "yongbao" ] && os_version=$(grep -E 'VERSION_ID=' /etc/os-release | awk -F'=' '{ print $2 }' | tr -d '"')

    # 分割版本号
    IFS='.' read -ra version_parts <<<"$os_version"

    # 提取主版本号和次版本号
    major_version=${version_parts[0]}
    minor_version=${version_parts[1]}
    if [ -z "$minor_version" ]; then
        minor_version=0
    fi
    # 通过 SHA384 文件来判断是否需要更新
    # ArchLinux特殊情况与一般情况（ArchLinux版本号为rolling，之前的判断方法会导致下载失败）
    case "$os_id" in
    "arch" | "manjaro" | "artix" | "chakra" | "blackarch" | "frugalware")
        wget "$base_url/basic/versions/8/16/$architecture/1/1/-1/0/" -O "$appVer_path" 2>&1
        ;;
    *)
        wget "$base_url/basic/versions/8/16/$architecture/$major_version/$minor_version/-1/0/" -O "$appVer_path" 2>&1
        ;;
    esac
    n_sha384=$(jq -r '.["\uD83E\uDD93"].Downloads[0].SHA384' "$appVer_path")

    downloads_url=$(jq -r '.["\uD83E\uDD93"].Downloads[0].DownloadUrl' "$appVer_path")

    # 检查 SHA384 值是否为空
    if [ "$n_sha384" = "" ]; then
        [ "$os_id" != "yongbao" ] && zenity --info --text="未知的最新版本 Hash:$n_sha384!" --width=300 || whiptail --msgbox "未知的最新版本 Hash:$n_sha384!" 10 60
        exit 244
    fi

    #本地版本 Hash
    if [ -f "AppVer" ]; then
        o_sha384=$(cat "AppVer")
    fi
    if [ -e "AppVer" ]; then
        if [ "${o_sha384,,}" = "${n_sha384,,}" ]; then
            Show_Run "已是最新版本，是否启动程序？"
            exit 0
        fi
    fi
}

Download_File() {
    # 删除旧的文件
    rm -rf $tar_path
    title="安装"
    # 检查 o_sha384 是否为空
    if [ -z "$o_sha384" ]; then
        title="安装"
    else
        title="更新"
    fi
    [ "$os_id" != "yongbao" ] && dialog1=zenity || dialog1=whiptail
    for i in {1..3}; do
        #下载文件到目标目录
        if [ "$os_id" != "yongbao" ]; then
            wget "$downloads_url" -O "$tar_path" 2>&1 | sed -u 's/.* \([0-9]\+%\)\ \+\([0-9.]\+.\) \(.*\)/\1\n# 下载中 \2\/s, 剩余时间： \3/' | zenity --progress --title="$title Watt Toolkit" --auto-close --width=500
        else
            wget "$downloads_url" -O "$tar_path" 2>&1 | sed -u 's/.* \([0-9]\+\)%.*/\1/' | whiptail --title "$title" --gauge "正在下载中" 10 60 0
        fi

        RUNNING=0
        while [ $RUNNING -eq 0 ]; do
            if [ -z "$(pidof $dialog1)" ]; then
                pkill wget
                RUNNING=1
            fi
            sleep 0.1
        done

        # 校验下载文件 Hash
        echo 正在校验哈希值
        actual_hash=$(sha384sum "$tar_name" | awk '{ print $1 }')
        if [ "${actual_hash,,}" = "${n_sha384,,}" ]; then
            rm "AppVer"
            echo "${actual_hash,,}" >>"$base_path/AppVer"
            break 2
        fi

        if [ "$i" -ge "3" ]; then
            [ "$os_id" != "yongbao" ] && zenity --error --text="下载错误。" --width=500 || whiptail --msgbox "下载错误。" 10 60
            exit 1
        fi
    done
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

Decompression() {
    echo "正在校验安装包"
    TOTAL_FILES=$(tar tf "$tar_name" 2>/dev/null | wc -l)
    {
       COUNTER=0
       tar -xzvf "$tar_name" 2>/dev/null | while read -r FILE; do
       COUNTER=$((COUNTER + 1))
       PERCENTAGE=$((COUNTER * 100 / TOTAL_FILES))
       echo "# 解压 $FILE"
       echo "$PERCENTAGE"
     done
     echo "100"
   }| { ([ "$os_id" != "yongbao" ] && zenity --progress --title="安装中" --text="正在解压文件..." --width=500 --percentage=0 --auto-close --no-cancel || whiptail --title "安装中" --gauge "正在解压文件..." 10 60 0)}
    rm -f "$appVer_path" &>/dev/null
    dotnet_path="$base_path/dotnet"
    dotnet_exec="$dotnet_path/dotnet"
    [ -x "$dotnet_exec" ] || chmod +x "$dotnet_exec"
    chmod +x "$base_path/$exec_name.sh"
}

#先安装依赖;
Install_wget
Install_certutil
[ "$os_id" != "yongbao" ] && Install_zenity || echo 勇豹没有包管理器，不能安装zenity，此处以whiptail代替
Install_jq
certutil_Init
#版本检查更新;
Get_NewVer

# 使用条件判断来检查文件是否存在
if [ -f "$tar_path" ]; then
    #如果本地存在文件与新版本计算 Hash 避免重复下载;
    temp_hash=$(sha384sum "$tar_path" | awk '{ print $1 }')
    if [ "${temp_hash,,}" != "${n_sha384,,}" ]; then
        #下载文件
        Download_File
    else
        rm "$base_path/AppVer"
        #版本号是最新缓存 输出到文件
        echo "${temp_hash,,}" >>"$base_path/AppVer"
        if [ "$os_id" != "yongbao" ]; then zenity --question --text="本地已有最新安装包是否继续解压?" --width=400; else whiptail --yesno "本地已有最新安装包是否继续解压?" 10 60; fi

        # 获取上一个命令的退出码
        response=$?

        if [ $response -eq 0 ]; then
            echo "继续解压"
        else
            # 用户点击了 "关闭" 按钮，退出脚本
            exit 0
        fi
    fi
else
    #下载文件
    Download_File
fi
#解压前尝试杀死旧版本进程
Kill_Process
#解压
Decompression
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
        choice=$([ "$os_id" != "yongbao" ] && { zenity --list --radiolist --title="请选择要添加到的位置" --column="选择" --column="路径" TRUE "$XDG_DESKTOP_DIR" FALSE "$HOME/.local/share/applications/";} || { whiptail --title "请选择要添加到的位置" --radiolist "" 10 60 2 "$XDG_DESKTOP_DIR" "" ON "$HOME/.local/share/applications/" "" OFF 3>&1 1>&2 2>&3;} )

        # 检查用户输入
        if [ "$choice" == "$HOME/.local/share/applications/" ]; then
            target_dir="$HOME/.local/share/applications/"
            break
        elif [ "$choice" == "$XDG_DESKTOP_DIR" ]; then
            target_dir="$XDG_DESKTOP_DIR"
            break
        else
            # 无效选项时给出提示，并继续循环
            [ "$os_id" != "yongbao" ] && zenity --info --text="无效选项，请重新选择。" --width=300 || whiptail --msgbox "无效选项，请重新选择。" 10 60
        fi
    done

    # 添加桌面文件
    rm -rf "$target_dir/Watt Toolkit.desktop" 2>/dev/null
    cat <<EOT > "$target_dir/Watt Toolkit.desktop"
[Desktop Entry]
Name=Watt Toolkit
Exec=$base_path/$exec_name.sh
Icon=$base_path/Icons/Watt-Toolkit.png
Terminal=false
Type=Application
StartupNotify=false
EOT
    chmod +x "$target_dir/Watt Toolkit.desktop"
}


InitDesktop
# update-desktop-database ~/.local/share/applications
#运行程序
if [ "$os_id" = "yongbao" ]; then sudo chmod u+s $(which pkexec); fi
Show_Run "下载安装完成，是否启动程序？"
exit 0
