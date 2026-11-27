#!/usr/bin/env bash
# Regression test: the desktop-entry script must write an absolute launcher path
# and create the desktop directory, even when started by a relative path from
# inside the script directory on a system without ~/Desktop.
set -euo pipefail
root=$(cd -- "$(dirname -- "$0")/.." && pwd)
work=$(mktemp -d)
trap 'rm -rf -- "$work"' EXIT

app="$work/WattToolkit"
mkdir -p "$app/script" "$work/home"
cp "$root/build/linux/init_desktop.sh" "$app/script/init_desktop.sh"
: > "$app/Steam++.sh"

(
    cd "$app/script"
    HOME="$work/home" bash init_desktop.sh
) >/dev/null

desktop="$work/home/Desktop/Watt Toolkit.desktop"
exec_line=$(grep '^Exec=' "$desktop")
expected="Exec=$app/Steam++.sh"
if [[ "$exec_line" != "$expected" ]]; then
    echo "expected $expected, got $exec_line" >&2
    exit 1
fi
echo "desktop launcher uses absolute path"
