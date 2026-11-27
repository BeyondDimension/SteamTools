#!/bin/bash
base_path="$HOME/WattToolkit"
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
    Determine_distribution
    Install_certutil
    certutil_Init
    exit 0
fi
