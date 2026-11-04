#!/bin/bash
default_base_path="$HOME/WattToolkit"
base_path="$default_base_path"
[ "$EUID" -eq 0 ] && { echo "此脚本不能以root身份执行。"; exit 1; }
command -v ldd &>/dev/null || { echo "Error: ldd tools is missing, please install ldd in your system."; exit 1; }
ldd --version 2>&1 | grep -qi 'musl' && { echo "Error: Watt Toolkit didn't support Linux distribution which use musl library, please use another tools." >&2; exit 1; } || echo "系统使用Glibc，继续运行..."
command -v dialog &>/dev/null && dialog1="dialog" || dialog1="whiptail"
while true; do
    if command -v zenity &>/dev/null; then
        custom_base_path=$(zenity --entry --title="安装路径" --text="请输入安装路径（默认为 "$default_base_path"，不输入则使用默认路径）")
    else
        custom_base_path=$($dialog1 --title "安装路径" --inputbox "请输入安装路径（默认为 "$default_base_path"，不输入则使用默认路径）" 10 60 3>&1 1>&2 2>&3)
    fi
    [ -n "$custom_base_path" ] && { base_path="$custom_base_path"; break; } || [ -z "$custom_base_path" ] && break
done
mkdir -p "$base_path" || { echo "无法创建安装路径 $base_path，请检查权限或路径是否正确。"; exit 1; }
cd "$base_path" || { echo "无法切换到安装路径 $base_path，请检查权限或路径是否正确。"; exit 1; }
appVer_path="$base_path/WattToolkit.AppVer"
exec_name="Steam++"
tar_name="WattToolkit.tgz"
tar_path="$base_path/$tar_name"
base_url="https://api.steampp.net"
architecture=1
app_name="Watt Toolkit"
title="下载 $app_name"
PROCESS_NAMES=("$exec_name" "$app_name")
Check_LC_Code() {
    current_lang="${LC_ALL:-$LANG}"
    echo "当前语言环境: $current_lang"
    if [ "$current_lang" = "C" ] || [ "$current_lang" = "C.UTF-8" ]; then
        echo "当前语言环境是 C 或 C.UTF-8，需要修改。"
        supported_locales=$(locale -a)
        [ -z "$supported_locales" ] && { echo "未能获取到支持的语言环境列表。请检查系统配置。"; return 1; }
        for locale in $supported_locales; do
            [ "$locale" != "C" ] && [ "$locale" != "C.UTF-8" ] && { export LC_ALL="$locale"; echo "语言环境已修改为 $locale。"; break; }
        done
    else
        echo "当前语言环境不是 C 或 C.UTF-8，无需修改。"
    fi
}
Check_LC_Code
Determine_distribution() {
    os_id=$(grep "^ID=" /etc/os-release|cut -d'=' -f2|tr -d '"'|tr '[:upper:]' '[:lower:]')
    echo "OS ID: $os_id"
    case "$os_id" in
    "ubuntu"|"debian"|"kali"|"mx"|"devuan"|"pureos"|"parrot"|"trisquel"|"bunsenlabs"|"deepin"|"antix"|"uos"|"kylin"|"openkylin"|"loongnix"|"gxde"|"nfsdesktop")
        installprefix="sudo apt install -y"; nssvar="libnss3-tools"; packageupdate="sudo apt update" ;;
    "fedora"|"neokylin") installprefix="sudo dnf install -y"; nssvar="nss-tools" ;;
    "centos"|"rhel"|"rocky"|"alma"|"amzn"|"alt") installprefix="sudo yum install -y"; nssvar="nss-tools";;
    "opensuse") installprefix="sudo zypper install"; nssvar="mozilla-nss-tools"; packageupdate="sudo zypper refresh" ;;
    "arch"|"manjaro"|"artix"|"chakra"|"blackarch"|"frugalware") installprefix="sudo pacman -Sy"; nssvar="nss" ;;
    "mageia"|"pclinuxos"|"openmandriva"|"rosa"|"vectorlinux") installprefix="sudo urpmi"; nssvar="nss-tools"; packageupdate="sudo urpmi.update -a";;
    "slackware"|"salix"|"porteus"|"slacko") installprefix="sudo slackpkg install"; nssvar="nss"; packageupdate="sudo slackpkg update gpg; sudo slackpkg update;";;
    "aosc") installprefix="sudo oma install -y"; nssvar="nss" ;;
    "gentoo") installprefix="sudo emerge -av"; nssvar="nss"; packageupdate="sudo emerge --sync" ;;
    "solus") installprefix="sudo eopkg install"; nssvar="nss-tools"; packageupdate="sudo eopkg update-repo" ;;
    *) manualins="1" ;;
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
        if [ "$os_id" == "gentoo" ]; then
             $installprefix net-misc/wget
        else
             $installprefix wget
        fi
        echo "wget 工具已安装。"
    fi
}
Install_certutil() {
    command -v certutil &>/dev/null && echo "certutil 工具已安装。" || { [ "$manualins" == "1" ] && echo "请手动安装 certutil 工具。" || { echo "证书导入以及验证需要使用 certutil 工具。";[-z "$packageupdate"]&&$packageupdate; $installprefix $nssvar;certutil_Init; [ "$os_id" == "loongnix" ] && { sudo ln -s /usr/sbin/setcap /usr/bin/setcap; sudo apt update; } || echo "certutil 工具已安装。"; } }
}
certutil_Init() {
    certutil -d $HOME/.pki/nssdb -L || { echo "即将初始化 certutil \$HOME/.pki/nssdb"; mkdir -p $HOME/.pki/nssdb; chmod 700 $HOME/.pki/nssdb; certutil -d $HOME/.pki/nssdb -N --empty-password; }
}
Install_jq() {
    command -v jq &>/dev/null && echo "jq 工具已安装。" || { [ "$manualins" == "1" ] && echo "请手动安装 jq 工具。" || { echo "jq 用来解析版本更新。";[-z "$packageupdate"]&&$packageupdate;$installprefix jq; echo "jq 工具已安装。"; } }
}
Install_zenity() {
    command -v zenity &>/dev/null && echo "zenity 工具已安装。" || { [ "$manualins" == "1" ] && echo "请手动安装 zenity 工具。" || { echo "安装过程需要 zenity 工具。";[-z "$packageupdate"]&&$packageupdate;$installprefix zenity;  echo "zenity 工具已安装。"; } }
}
Show_Run() {
    if [ "$os_id" != "yongbao" ]; then zenity --question --text="$1" --width=400; else whiptail --yesno "$1" 10 60; fi
    [ $? -eq 0 ] && { echo "程序已启动。"; exit 0 & /bin/sh -c "$base_path/$exec_name.sh"; } || exit 0
}
Get_NewVer() {
    arch=$(uname -m)
    case $arch in
    x86_64) architecture=1 ;;
    aarch64) architecture=3 ;;
    loongarch64 | loong64) architecture=6 ;;
    i?86 | arm*) zenity --info --text="Watt Toolkit不再支持32位，32位用户请自行在Github/Gitee下载旧版使用，谢谢。" --width=300; exit 244 ;;
    *) [ "$os_id" != "yongbao" ] && zenity --info --text="未知的设备架构:$arch!" --width=300 || whiptail --msgbox "未知的设备架构:$arch!" 10 60; exit 244 ;;
    esac
    [ "$os_id" != "yongbao" ] && os_version=$(grep -E 'VERSION_ID=' /etc/os-release | awk -F'=' '{ print $2 }' | tr -d '"') || os_version=$(grep -E 'VERSION=' /etc/os-release | awk -F'=' '{ print $2 }' | tr -d '"')
    [ -z "$os_version" ] && os_version=$(grep -E 'BUILD_ID=' /etc/os-release | awk -F'=' '{ print $2 }' | tr -d '"')
    IFS='.' read -ra version_parts <<<"$os_version"
    major_version=${version_parts[0]}
    minor_version=${version_parts[1]:-0}
    case "$os_id" in
    "arch" | "manjaro" | "artix" | "chakra" | "blackarch" | "frugalware") wget "$base_url/basic/versions/8/16/$architecture/1/1/-1/0/" -O "$appVer_path" 2>&1 ;;
    *) wget "$base_url/basic/versions/8/16/$architecture/$major_version/$minor_version/-1/0/" -O "$appVer_path" 2>&1 ;;
    esac
    n_sha384=$(jq -r '.["\uD83E\uDD93"].Downloads[0].SHA384' "$appVer_path")
    downloads_url=$(jq -r '.["\uD83E\uDD93"].Downloads[0].DownloadUrl' "$appVer_path")
    [ -z "$n_sha384" ] && { [ "$os_id" != "yongbao" ] && zenity --info --text="未知的最新版本 Hash:$n_sha384!" --width=300 || whiptail --msgbox "未知的最新版本 Hash:$n_sha384!" 10 60; exit 244; }
    [ -f "AppVer" ] && o_sha384=$(cat "AppVer")
    [ -e "AppVer" ] && [ "${o_sha384,,}" = "${n_sha384,,}" ] && { Show_Run "已是最新版本，是否启动程序？"; exit 0; }
}
Download_File() {
    rm -rf $tar_path
    [ "$os_id" != "yongbao" ] && dialog1=zenity || dialog1=whiptail
    for i in {1..3}; do
        if [ "$os_id" != "yongbao" ]; then
            wget "$downloads_url" -O "$tar_path" 2>&1 | sed -u 's/.* \([0-9]\+%\)\ \+\([0-9.]\+.\) \(.*\)/\1\n# 下载中 \2\/s, 剩余时间： \3/' | zenity --progress --title="$title" --auto-close --width=500
        else
            wget "$downloads_url" -O "$tar_path" 2>&1 | sed -u 's/.* \([0-9]\+\)%.*/\1/' | whiptail --title "$title" --gauge "正在下载中" 10 60 0
        fi
        RUNNING=0
        while [ $RUNNING -eq 0 ]; do
            [ -z "$(pidof $dialog1)" ] && { pkill wget; RUNNING=1; }
            sleep 0.1
        done
        echo 正在计算哈希值
        actual_hash=$(sha384sum "$tar_name" | awk '{ print $1 }')
        [ "${actual_hash,,}" = "${n_sha384,,}" ] && { rm "AppVer"; echo "${actual_hash,,}" >>"$base_path/AppVer"; break 2; }
        [ "$i" -ge "3" ] && { [ "$os_id" != "yongbao" ] && zenity --error --text="下载错误。" --width=500 || whiptail --msgbox "下载错误。" 10 60; exit 1; }
    done
}
Kill_Process() {
    Kill_MAX_RETRIES=3
    for process_name in "${PROCESS_NAMES[@]}"; do
        kill_retry=1
        while [ $kill_retry -le $Kill_MAX_RETRIES ]; do
            pid=$(pgrep "$process_name")
            [ -n "$pid" ] && { echo "尝试 $kill_retry: 进程 $process_name 正在运行中。正在终止..."; kill $pid; sleep 2; } || break
            kill_retry=$((kill_retry + 1))
        done
    done
    for process_name in "${PROCESS_NAMES[@]}"; do
        pgrep -x "$process_name" >/dev/null && { echo "无法终止程序 $process_name。尝试次数已达上限。"; exit 1; } || echo "程序 $process_name 已成功终止。"
    done
}
show_progress() {
if [ "$os_id" != "yongbao" ]; then
    zenity --progress --title="$app_name" --text="获取压缩包信息中..." --width=500 --auto-close --no-cancel
else
    whiptail --gauge "$app_name" 10 60 0
fi
}
Decompression() { 
    echo "正在校验安装包"
    {
       TOTAL_FILES=$(tar tf "$tar_name" 2>/dev/null | wc -l)
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
Install_wget
Install_certutil
[ "$os_id" != "yongbao" ] && Install_zenity || echo 勇豹没有包管理器，不能安装zenity，此处以whiptail代替
Install_jq
Get_NewVer
if [ -f "$tar_path" ]; then
    temp_hash=$(sha384sum "$tar_path" | awk '{ print $1 }')
    [ "${temp_hash,,}" != "${n_sha384,,}" ] && Download_File || { rm "$base_path/AppVer"; echo "${temp_hash,,}" >>"$base_path/AppVer"; if [ "$os_id" != "yongbao" ]; then zenity --question --text="本地已有最新安装包是否继续解压?" --width=400; else whiptail --yesno "本地已有最新安装包是否继续解压?" 10 60; fi; [ $? -eq 0 ] && echo "继续解压" || exit 0; }
else
    Download_File
fi
Kill_Process
Decompression
InitDesktop() {
    XDG_DESKTOP_DIR=$(command -v xdg-user-dir &>/dev/null && xdg-user-dir DESKTOP || echo "$HOME/Desktop")
    while true; do
        choice=$([ "$os_id" != "yongbao" ] && { zenity --list --radiolist --title="请选择要添加到的位置" --column="选择" --column="路径" TRUE "$XDG_DESKTOP_DIR" FALSE "$HOME/.local/share/applications/";} || { whiptail --title "请选择要添加到的位置" --radiolist "" 10 60 2 "$XDG_DESKTOP_DIR" "" ON "$HOME/.local/share/applications/" "" OFF 3>&1 1>&2 2>&3;} )
        [ "$choice" == "$HOME/.local/share/applications/" ] && { target_dir="$HOME/.local/share/applications/"; break; } || [ "$choice" == "$XDG_DESKTOP_DIR" ] && { target_dir="$XDG_DESKTOP_DIR"; break; } || [ "$os_id" != "yongbao" ] && zenity --info --text="无效选项，请重新选择。" --width=300 || whiptail --msgbox "无效选项，请重新选择。" 10 60
    done
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
if [ "$os_id" = "yongbao" ]; then sudo chmod u+s $(which pkexec); fi
Show_Run "下载安装完成，是否启动程序？"
exit 0
