create_app_structure() {
    APPNAME=$1
    APPDIR="$APPNAME.app/Contents"
    APPICONS="/Users/mossimo/bin/logo.icns"

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
	<string>2.4.9</string>
	<key>CFBundleVersion</key>
	<string>2.4.9.0</string>
	<key>LSMinimumSystemVersion</key>
	<string>10.13</string>
	<key>CFBundleDevelopmentRegion</key>
	<string>zh_CN</string>
	<key>CFBundleInfoDictionaryVersion</key>
	<string>6.0</string>
    <key>CFBundleExecutable</key>
    <string>$PLIST_APPNAME</string>
    <key>CFBundleGetInfoString</key>
    <string>$PLIST_APPNAME</string>
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
cd "/Users/mossimo/bin"


BINARYNAME="Steam++"
CEFZIP="cef_binary_90.6.3%2Bgc53c523%2Bchromium-90.0.4430.93_macosx64_minimal.tar.bz2"


echo "Building CefNet Avalonia demo..."

CEFBINARIES="../cef"
CEFFRAMEWORK_DIR="$(find $CEFBINARIES -name "Release")"

if [ ! -d "$CEFFRAMEWORK_DIR" ]; then
    if [ ! -f "$CEFBINARIES/$CEFZIP" ]; then
        echo "downloading cef binaries from https://cef-builds.spotifycdn.com/$CEFZIP"
        curl -o "$CEFBINARIES/$CEFZIP" "https://cef-builds.spotifycdn.com/$CEFZIP"
    fi
    echo "unzipping cef binaries"
    tar -jxvf "$CEFBINARIES/$CEFZIP" --strip-components 1 -C "./$CEFBINARIES"
    CEFFRAMEWORK_DIR="$(find $CEFBINARIES -name "Release")"
fi


APPNAME="$BINARYNAME"
APPDIR="$APPNAME.app/Contents"

rm -rf "$APPDIR"

create_app_structure "$APPNAME"
emit_plist "$APPNAME" "$APPDIR" true

cp -R "$CEFFRAMEWORK_DIR/Chromium Embedded Framework.framework" "$APPDIR/Frameworks/"

cd "$APPDIR/Frameworks"

APPNAME="$BINARYNAME Helper"
APPDIR="$APPNAME.app/Contents"
create_app_structure "$APPNAME"
emit_plist "$APPNAME" "$APPDIR"
cp -R "../../../steampp/" "$APPDIR/MacOS"
ln -s "Frameworks/$APPDIR/MacOS" "../MacOS"
chmod +x "$APPDIR/MacOS/$BINARYNAME"
cp "$APPDIR/MacOS/$BINARYNAME" "$APPDIR/MacOS/$APPNAME"
chmod +x "$APPDIR/MacOS/$APPNAME"

APPNAME="$BINARYNAME Helper (GPU)"
APPDIR="$APPNAME.app/Contents"
mkdir -pv "$APPDIR"
emit_plist "$APPNAME" "$APPDIR"
ln -s "../../$BINARYNAME Helper.app/Contents/MacOS" "$APPDIR/MacOS"
cp "$APPDIR/MacOS/$BINARYNAME" "$APPDIR/MacOS/$APPNAME"
chmod +x "$APPDIR/MacOS/$APPNAME"

APPNAME="$BINARYNAME Helper (Plugin)"
APPDIR="$APPNAME.app/Contents"
mkdir -pv "$APPDIR"
emit_plist "$APPNAME" "$APPDIR"
ln -s "../../$BINARYNAME Helper.app/Contents/MacOS" "$APPDIR/MacOS"
cp "$APPDIR/MacOS/$BINARYNAME" "$APPDIR/MacOS/$APPNAME"
chmod +x "$APPDIR/MacOS/$APPNAME"

APPNAME="$BINARYNAME Helper (Renderer)"
APPDIR="$APPNAME.app/Contents"
mkdir -pv "$APPDIR"
emit_plist "$APPNAME" "$APPDIR"
ln -s "../../$BINARYNAME Helper.app/Contents/MacOS" "$APPDIR/MacOS"
cp "$APPDIR/MacOS/$BINARYNAME" "$APPDIR/MacOS/$APPNAME"
chmod +x "$APPDIR/MacOS/$APPNAME"
