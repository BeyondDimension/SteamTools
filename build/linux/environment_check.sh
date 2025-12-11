#!/bin/bash
base_path="$HOME/WattToolkit"
export LC_ALL=en_US.UTF-8
if [ "$1" = "-c" ]; then
    # 只验证 certutil 是否可用
    if command -v certutil &>/dev/null; then
        exit 200
    else
        echo "请手动安装 certutil 工具。"
        exit 1
    fi
else
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
    Install_certutil
    certutil_Init
    exit 0
fi
