namespace Mobius.Enums;

/// <summary>
/// 下载迅游加速器回调的返回值
/// <para>1.接口 xunyou_install</para>
/// </summary>
public enum XunYouDownLoadCode
{
    /// <summary>
    /// 专版迅游安装成功。
    /// <list type="number">
    /// <item>迅游正在运行，并弹出迅游的客户端界面。</item>
    /// <item>迅游未运行，但专版已经安装，直接运行专版迅游进行加速。</item>
    /// <item>下载专版安装包成功。</item>
    /// </list>
    /// </summary>
    安装成功 = 101,

    /// <summary>
    /// 下载失败
    /// <list type="bullet">
    /// <item>场景一：如果专版正在下载，若再次下载返回该值。</item>
    /// <item>场景二：下载专版安装包失败。</item>
    /// </list>
    /// </summary>
    下载失败 = 102,

    /// <summary>
    /// 执行安装程序失败
    /// </summary>
    执行安装程序失败 = 103,

    /// <summary>
    /// 新建下载内存空间失败
    /// </summary>
    新建下载内存空间失败 = 104,

    /// <summary>
    /// 创建 Http 流失败，请查阅是否屏蔽了 host
    /// </summary>
    创建Http流失败 = 201,

    /// <summary>
    /// 获取专版迅游的下载地址失败
    /// </summary>
    获取专版迅游的下载地址失败 = 202,

    /// <summary>
    /// 下载地址解析错误
    /// </summary>
    下载地址解析错误 = 203,

    /// <summary>
    /// 启动安装程序失败
    /// </summary>
    启动安装程序失败 = 300,
}