create_app_structure() {
    APPNAME=$1
    APPDIR="$APPNAME.app/Contents"
    APPICONS="${{ Steam++_IcnsFile }}"

    if [ ! -d "$APPDIR" ]; then
        echo "creating app structure $APPDIR"

        mkdir -vp "$APPDIR"/{Resources,Frameworks}
        cp -v "$APPICONS" "$APPDIR/Resources/$APPNAME.icns"
    fi
}

emit_plist() {
    PLIST_APPNAME=$1
    PLIST_PATH="$2/Info.plist"
    
    if [ "$3" ]; then
        LSUIELEMENT="false"
    else
        LSUIELEMENT="true"
    fi
    
    if [ ! -f "$PLIST_PATH" ]; then
        echo "emiting $PLIST_PATH"
        cat <<EOF > "$PLIST_PATH"
<?xml version="1.0" encoding="UTF-8"?>
<!DOCTYPE plist PUBLIC "-//Apple//DTD PLIST 1.0//EN" "http://www.apple.com/DTDs/PropertyList-1.0.dtd">
<plist version="1.0">
<dict>
	<key>CFBundleName</key>
	<string>$PLIST_APPNAME</string>
	<key>CFBundleIdentifier</key>
	<string>net.steampp.app</string>
	<key>CFBundleShortVersionString</key>
	<string>${{ Steam++_ShortVersion }}</string>
	<key>CFBundleVersion</key>
	<string>${{ Steam++_Version }}</string>
	<key>LSMinimumSystemVersion</key>
	<string>10.15</string>
	<key>CFBundleDevelopmentRegion</key>
	<string>zh_CN</string>
	<key>CFBundleInfoDictionaryVersion</key>
	<string>6.0</string>
	<key>CFBundleExecutable</key>
	<string>${{ Steam++_RunName }}</string>
	<key>CFBundleGetInfoString</key>
	<string>${{ Steam++_RunName }}</string>
	<key>CFBundleIconFile</key>
	<string>$PLIST_APPNAME</string>
	<key>CFBundlePackageType</key>
	<string>APPL</string>
	<key>CFBundleSignature</key>
	<string>????</string>
	<key>LSUIElement</key>
	<string>$LSUIELEMENT</string>
	<key>NSHumanReadableCopyright</key>
	<string>© 长沙次元超越科技有限公司. All Rights Reserved.</string>
	<key>NSPrincipalClass</key>
	<string>NSApplication</string>
</dict>
</plist>
EOF
    fi
}

cd "${{ Steam++_OutPutFilePath }}"


BINARYNAME="${{ Steam++_AppName }}"

echo "Building Avalonia demo..."


APPNAME="$BINARYNAME"
APPDIR="$APPNAME.app/Contents"

rm -rf "$APPDIR"

create_app_structure "$APPNAME"
emit_plist "$APPNAME" "$APPDIR" true

mv -f "${{ Steam++_APPDIR }}" "$APPDIR/MacOS"
chmod +x "$APPDIR/MacOS/Steam++"

mkdir "${{ Steam++_APPDIR }}"
mv -f "${{ Steam++_OutPutFilePath }}/$BINARYNAME.app" "${{ Steam++_APPDIR }}/$BINARYNAME.app"