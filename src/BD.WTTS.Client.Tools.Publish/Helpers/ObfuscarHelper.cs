namespace BD.WTTS.Client.Tools.Publish.Helpers;

static class ObfuscarHelper
{
    // dotnet tool install --global Obfuscar.GlobalTool

    const string configFileName = "Obfuscar.xml";

    static void WriteConfig(string dirPath)
    {
        var xml =
$"""
<?xml version='1.0'?>
<Obfuscator>
  <!-- 输入的工作路径，采用如约定的 Windows 下的路径表示法，如以下表示当前工作路径 -->
  <!-- 推荐使用当前工作路径，因为 DLL 的混淆过程，需要找到 DLL 的所有依赖。刚好当前工作路径下，基本都能满足条件 -->
  <Var name="InPath" value="." />
  <!-- 混淆之后的输出路径，如下面代码，设置为当前工作路径下的 Obfuscar 文件夹 -->
  <!-- 混淆完成之后的新 DLL 将会存放在此文件夹里 -->
  <Var name="OutPath" value=".\Obfuscar" />
  <!-- 以下的都是细节的配置，配置如何进行混淆 -->

  <!-- 使用 KeepPublicApi 配置是否保持公开的 API 不进行混淆签名，如公开的类型公开的方法等等，就不进行混淆签名了 -->
  <!-- 语法的写法就是 name 表示某个开关，而 value 表示值 -->
  <!-- 对于大部分的库来说，设置公开的 API 不进行混淆是符合预期的 -->
  <Var name="KeepPublicApi" value="false" />
  <!-- 设置 HidePrivateApi 为 true 表示，对于私有的 API 进行隐藏，隐藏也就是混淆的意思 -->
  <!-- 可以通过后续的配置，设置混淆的方式，例如使用 ABC 字符替换，或者使用不可见的 Unicode 代替 -->
  <Var name="HidePrivateApi" value="true" />
  <!-- 设置 HideStrings 为 true 可以设置是否将使用的字符串进行二次编码 -->
  <!-- 由于进行二次编码，将会稍微伤一点点性能，二次编码需要在运行的时候，调用 Encoding 进行转换为字符串 -->
  <Var name="HideStrings" value="false" />
  <!-- 设置 UseUnicodeNames 为 true 表示使用不可见的 Unicode 字符代替原有的命名，通过此配置，可以让反编译看到的类和命名空间和成员等内容都是不可见的字符 -->
  <Var name="UseUnicodeNames" value="false" />
  <Var name="UseKoreanNames" value="true" />
  <!-- 是否复用命名，设置为 true 的时候，将会复用命名，如在不同的类型里面，对字段进行混淆，那么不同的类型的字段可以是重名的 -->
  <!-- 设置为 false 的时候，全局将不会有重复的命名 -->
  <Var name="ReuseNames" value="true" />
  <!-- 配置是否需要重命名字段，默认配置了 HidePrivateApi 为 true 将都会打开重命名字段，因此这个配置的存在只是用来配置为 false 表示不要重命名字段 -->
  <Var name="RenameFields" value="true" />
  <!-- 是否需要重新生成调试信息，生成 PDB 符号文件 -->
  <Var name="RegenerateDebugInfo" value="false" />
  <Var name="OptimizeMethods" value="true" />
  <Var name="SuppressIldasm" value="true" />
  <Var name="KeyFile" value="{Path.Combine(ProjectUtils.ProjPath, "WattToolkit.snk")}" />
  <!-- 需要进行混淆的程序集，可以传入很多个，如传入一排排 -->
  <!-- <Module file="$(InPath)\Lib1.dll" /> -->
  <!-- <Module file="$(InPath)\Lib2.dll" /> -->
  <Module file="$(InPath)\Steam++.exe" />
</Obfuscator>
""";

        File.WriteAllBytes(Path.Combine(dirPath, configFileName), Encoding.UTF8.GetBytes(xml));
    }

    public static void Start(string dirPath)
    {
        WriteConfig(dirPath);

        var psi = DotNetCLIHelper.GetProcessStartInfo(AppContext.BaseDirectory);
        psi.Arguments = "tool install --global Obfuscar.GlobalTool";
        ProcessHelper.StartAndWaitForExit(psi, ignoreExitCode: true);

        psi = new ProcessStartInfo
        {
            FileName = "obfuscar.console.exe",
            UseShellExecute = false,
            Arguments = configFileName,
            WorkingDirectory = dirPath,
        };
        ProcessHelper.StartAndWaitForExit(psi);

        var oldFilePath = Path.Combine(dirPath, "Steam++.exe");
        var newFilePath = Path.Combine(dirPath, "Obfuscar", "Steam++.exe");
        File.Delete(oldFilePath);
        File.Move(newFilePath, oldFilePath);
    }
}
