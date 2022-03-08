namespace System.Properties
{
    static partial class ThisAssembly
    {
        /// <summary>
        /// 版本号
        /// <para>在以下平台中还需要将版本号写入相关的清单文件：</para>
        /// <list type="bullet">
        ///   <item> iOS：\src\ST.Client.Mobile.iOS.App\Info.plist &lt;CFBundleVersion&gt; &lt;CFBundleShortVersionString&gt; </item>
        ///   <item> Android：\src\ST.Client.Mobile.Droid.App\Properties\AndroidManifest.xml &lt;manifest ... android:versionCode(long递增) android:versionName </item>
        ///   <item> UWP(DesktopBridge)：\src\ST.Client.Desktop.Avalonia.App.Bridge.Package\Package.appxmanifest &lt;Identity ... Version </item>
        ///   <item> macOS：\packaging\build-osx-app.sh &lt;CFBundleVersion&gt; &lt;CFBundleShortVersionString&gt; </item>
        /// </list>
        /// </summary>
        public const string Version = "2.6.9";

        //public const string InfoVersion = Version + "-beta";
    }
}