#!/bin/bash

set -euo pipefail

# 获取CPU架构
arch=$(uname -m)

# 检查CPU指令集扩展
case $arch in
    x86_64)
        # 检查是否支持SSE4.2
        if grep -iq 'sse4_2' /proc/cpuinfo; then
            echo "该CPU及Linux发行版支持SSE4.2扩展指令集，可以正常运行Watt Toolkit"
        else
            echo "该CPU或Linux发行版不支持SSE4.2扩展指令集，可能不能正常运行Watt Toolkit！"
            exit 1
        fi
        ;;
    aarch64 | arm64)
        # 检查是否支持NEON
        if grep -iq -e 'neon' -e 'asimd' /proc/cpuinfo; then
            echo "该CPU及Linux发行版支持ARM SIMD扩展指令集（NEON/ASIMD），可以正常运行Watt Toolkit"
        else
            echo "该CPU或Linux发行版不支持ARM SIMD扩展指令集（NEON/ASIMD），可能不能正常运行Watt Toolkit！"
            exit 1
        fi
        ;;
    loongarch64)
        # 检查是否支持LSX和LASX，Watt Toolkit龙芯版的运行同时需要LSX与LASX。先检查Linux内核是否大于6.6
        major=$(uname -r | awk -F '[.-]' '{print $1}')
        minor=$(uname -r | awk -F '[.-]' '{print $2}' | sed 's/[^0-9].*//')
        minor=${minor:-0}
        if [ "$major" -gt 6 ] || { [ "$major" -eq 6 ] && [ "$minor" -ge 6 ]; }; then
            if grep -iq 'lsx' /proc/cpuinfo && grep -iq 'lasx' /proc/cpuinfo; then
                echo "该CPU及Linux发行版同时支持LSX和LASX扩展指令集，可以正常运行Watt Toolkit"
            else
                echo "Watt Toolkit所需的.NET需要强制LSX或LASX，该CPU至少缺LSX或LASX不满足要求。"
                exit 1
            fi
        else
            echo "内核版本<6.6，不足以运行Watt Toolkit新世界发行版"
            exit 1
        fi
        ;;
    riscv64)
        echo "由于绝大部分RISC-V 64开发版不支持RVV 1.0指令，且检测该指令命令复杂，此处不检测。"
        ;;
    i?86 | arm*)
        # 32位不再受到支持
        echo "Watt Toolkit不再支持32位，32位用户请自行在Github/Gitee下载旧版使用，谢谢。"
        exit 1
        ;;
    *)
        echo "未知CPU架构：$arch"
        exit 1
        ;;
esac
