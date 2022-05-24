namespace System.Properties;

public static partial class ThisAssembly
{
    /// <summary>
    /// 定义程序集清单的产品名自定义属性
    /// </summary>
    public const string AssemblyProduct = "Watt Toolkit";

    /// <summary>
    /// 定义程序集清单的商标自定义属性
    /// </summary>
    public const string AssemblyTrademark = "Watt Toolkit";

    /// <summary>
    /// 提供程序集的文本说明
    /// </summary>
    public const string AssemblyDescription = "「Watt Toolkit」是一个开源跨平台的多功能游戏工具箱。";

    /// <summary>
    /// 定义程序集清单的版权自定义属性
    /// </summary>
    public const string AssemblyCopyright = "© 长沙次元超越科技有限公司. All rights reserved.";

    /// <summary>
    /// 定义程序集清单的公司名称自定义属性
    /// </summary>
    public const string AssemblyCompany = "长沙次元超越科技有限公司";

    public const string PublicKey =
#if SIGN_ASSEMBLY
        ", PublicKey=" +
        "002400000480000094000000060200000024000052534131000400000100010029b4f7706cbb7e23b3cf33be41127d4b" +
        "12c14a77cc1094412e73ccbbea4fbc873883042b5a9e517df99137f31f610624f79b46980bcceb990db3caa619fbbb31989f" +
        "5b6db3689d99ff1f70cb9bc20cc4d548beb942e09859cf2c0690683c9ad160a4a7287070e9e49795c75ba3d12723ddb4" +
        "ddeb11d32f193e0882db10b41de3";
#else
        "";
#endif

    public const bool Debuggable =
#if DEBUG
true
#else
false
#endif
        ;
}