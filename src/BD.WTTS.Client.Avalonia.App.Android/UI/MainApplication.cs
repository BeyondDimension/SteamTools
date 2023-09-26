namespace BD.WTTS.Droid.UI;

[Register(AssemblyInfo.JavaPkgNames.UI + nameof(MainApplication))]
[Application]
public sealed partial class MainApplication : AndroidApplication
{
    public static MainApplication Instance =>
        Context is MainApplication app ? app :
        throw new ArgumentNullException(nameof(app));

    public MainApplication(IntPtr javaReference, JniHandleOwnership transfer)
        : base(javaReference, transfer)
    {

    }

    public override void OnCreate()
    {
        base.OnCreate();

        bool GetIsMainProcess()
        {
            // 注意：进程名可以自定义，默认是包名，如果自定义了主进程名，这里就有误，所以不要自定义主进程名！
            var name = this.GetCurrentProcessName();
            return name == PackageName;
        }
        var isMainProcess = GetIsMainProcess();

        Program.Main(Array.Empty<string>());
    }
}
