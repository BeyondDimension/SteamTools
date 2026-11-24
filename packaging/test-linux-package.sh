#!/usr/bin/env bash
# Validate the Linux launcher in both deployment modes and, optionally, a published tarball.
set -euo pipefail
root=$(cd -- "$(dirname -- "$0")/.." && pwd)
work=$(mktemp -d)
trap 'rm -rf -- "$work"' EXIT

case_dir="$work/package with spaces"
mkdir -p "$case_dir"
cp "$root/build/linux/Steam++.sh" "$case_dir/Steam++.sh"
cat > "$case_dir/Steam++" <<'APP'
#!/bin/bash
printf 'native: %s\n' "$1"
APP
chmod +x "$case_dir/Steam++"
cp "$case_dir/Steam++" "$work/native-original"
[[ $(bash "$case_dir/Steam++.sh" "some argument") == 'native: some argument' ]]
cmp "$work/native-original" "$case_dir/Steam++"
[[ ! -L "$case_dir/Steam++" ]]
rm -- "$case_dir/Steam++"

mkdir -p "$case_dir/dotnet" "$case_dir/assemblies"
touch "$case_dir/assemblies/Steam++.dll"
cat > "$case_dir/dotnet/dotnet" <<'DOTNET'
#!/bin/bash
printf 'managed: %s %s\n' "$(basename -- "$1")" "$2"
DOTNET
chmod +x "$case_dir/dotnet/dotnet"
[[ $(bash "$case_dir/Steam++.sh" "some argument") == 'managed: Steam++.dll some argument' ]]
[[ -L "$case_dir/Steam++" ]]

desktop-file-validate "$root/build/linux/installWattToolkit.desktop"
if (( $# > 0 )); then
    archive=$(realpath -- "$1")
    package="$work/archive"
    mkdir "$package"
    tar -xzf "$archive" -C "$package"
    cmp "$root/res/icons/app/v3/Logo_512.png" "$package/Icons/Watt-Toolkit.png"
    [[ -x "$package/Steam++.sh" ]]
    cmp "$root/build/linux/Steam++.sh" "$package/Steam++.sh"
    for script in "$root"/build/linux/*.sh; do
        [[ $(basename "$script") == Steam++.sh ]] && continue
        filename=$(basename "$script")
        [[ -x "$package/script/$filename" ]]
        cmp "$script" "$package/script/$filename"
    done
    [[ -x "$package/script/installWattToolkit.desktop" ]]
    cmp "$root/build/linux/installWattToolkit.desktop" "$package/script/installWattToolkit.desktop"
fi
echo 'Linux package checks passed.'
