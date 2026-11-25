#!/bin/bash
set -euo pipefail

if [[ $# -ne 3 ]]; then
    echo "Usage: bash create-dmg.sh <template.dmg> <Steam++.app> <output.dmg>" >&2
    exit 2
fi

template=$1
app_bundle=$2
output=$3
work=$(mktemp -d "$(dirname "$output")/.installer-layout.XXXXXX")
mount_point="$work/volume"
layout="$work/layout"
mounted=

cleanup() {
    if [[ -n "$mounted" ]] && ! /usr/bin/hdiutil detach "$mount_point" -quiet; then
        echo "Could not detach the installer volume at $mount_point" >&2
        return
    fi
    rm -rf -- "$work"
}
trap cleanup EXIT
mkdir "$mount_point" "$layout"

# A private copy allows packaging even when the original is open in Finder.
/bin/cp -c "$template" "$work/template.dmg"
/usr/bin/hdiutil attach "$work/template.dmg" -readonly -nobrowse -noautoopen -quiet -mountpoint "$mount_point"
mounted=1
volume_name=$(/usr/sbin/diskutil info -plist "$mount_point" | /usr/bin/plutil -extract VolumeName raw -o - -)
finder_info=$(/usr/bin/xattr -px com.apple.FinderInfo "$mount_point" | tr -d '[:space:]')

# Carry the Finder layout and artwork without copying the old bundle or volume bookkeeping.
for asset in .DS_Store .VolumeIcon.icns b.png l.icns; do
    if [[ ! -f "$mount_point/$asset" || -L "$mount_point/$asset" ]]; then
        echo "Installer template is missing the Finder layout asset $asset" >&2
        exit 1
    fi
    /usr/bin/ditto --noqtn "$mount_point/$asset" "$layout/$asset"
done
/usr/bin/hdiutil detach "$mount_point" -quiet
mounted=

# Keep the template's label and icon position with a shortcut that works on any Mac.
ln -s /Applications "$layout/应用程序"
# Only remove quarantine when the input bundle or one of its files is marked.
sudo xattr -rd com.apple.quarantine "$app_bundle"

/usr/bin/ditto "$app_bundle" "$layout/Steam++.app"

working_image="$work/layout.dmg"
/usr/bin/hdiutil create -quiet -srcfolder "$layout" -volname "$volume_name" -fs APFS -format UDRW "$working_image"
/usr/bin/hdiutil attach "$working_image" -readwrite -nobrowse -noautoopen -quiet -mountpoint "$mount_point"
mounted=1
# hdiutil copies the files, but not the volume root's custom icon flag.
/usr/bin/xattr -wx com.apple.FinderInfo "$finder_info" "$mount_point"
/usr/bin/codesign --verify --deep --strict "$mount_point/Steam++.app"
/usr/bin/hdiutil detach "$mount_point" -quiet
mounted=

/usr/bin/hdiutil convert "$working_image" -quiet -format UDZO -o "$output"
/usr/bin/hdiutil verify -quiet "$output"
