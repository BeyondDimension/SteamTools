#!/bin/bash
base_path="$HOME/WattToolkit"
export LC_ALL=en_US.UTF-8
exec_name="Steam++"
app_name="Watt Toolkit"
AppData="$XDG_DATA_HOME/Steam++"
AppData_t="$HOME/.local/share/Steam++"
Cache="$XDG_CACHE_HOME/Steam++"
Cache_t="$HOME/.cache/Steam+"
PROCESS_NAMES=("$exec_name" "$app_name")

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
Kill_Process

zenity --question --text="是否删除证书?" --width=400
# 获取上一个命令的退出码
delCer=$?

if [ $delCer -eq 0 ]; then
    certutil -D -d $HOME/.pki/nssdb -n "SteamTools"
fi

zenity --question --text="是否删除用户数据?" --width=400
# 获取上一个命令的退出码
delUserData=$?

if [ $delUserData -eq 0 ]; then
    rm -rf $AppData 2>/dev/null
    rm -rf $AppData_t 2>/dev/null
    certutil -D -d $HOME/.pki/nssdb -n "SteamTools"
else
    echo "保留用户数据方便下次安装。"
    echo "如需删除可手动删除该目录:$AppData 或者 $AppData_t"
fi
rm -rf $Cache 2>/dev/null
rm -rf $Cache_t 2>/dev/null
rm -rf $base_path 2>/dev/null
rm -rf "$HOME/Desktop/Watt Toolkit.desktop" 2>/dev/null
zenity --info --text="卸载完成!" --width=300
