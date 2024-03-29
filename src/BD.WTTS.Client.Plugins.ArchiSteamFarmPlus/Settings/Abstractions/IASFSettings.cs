#nullable enable
#pragma warning disable IDE0079 // 请删除不必要的忽略
#pragma warning disable SA1634 // File header should show copyright
#pragma warning disable CS8601 // 引用类型赋值可能为 null。
#pragma warning disable CS0108 // 成员隐藏继承的成员；缺少关键字 new
//------------------------------------------------------------------------------
// <auto-generated>
//     此代码由包 BD.Common.Settings.V4.SourceGenerator.Tools 源生成。
//
//     对此文件的更改可能会导致不正确的行为，并且如果
//     重新生成代码，这些更改将会丢失。
// </auto-generated>
//------------------------------------------------------------------------------

// ReSharper disable once CheckNamespace
namespace BD.WTTS.Settings.Abstractions;

public partial interface IASFSettings
{
    static IASFSettings? Instance
        => Ioc.Get_Nullable<IOptionsMonitor<IASFSettings>>()?.CurrentValue;

    /// <summary>
    /// ASF 路径
    /// </summary>
    string? ArchiSteamFarmExePath { get; set; }

    /// <summary>
    /// 程序启动时自动运行 ASF
    /// </summary>
    bool AutoRunArchiSteamFarm { get; set; }

    /// <summary>
    /// 检查文件安全性
    /// </summary>
    bool CheckArchiSteamFarmExe { get; set; }

    /// <summary>
    /// 控制台默认最大行数
    /// </summary>
    int ConsoleMaxLineDefault { get; set; }

    /// <summary>
    /// 控制台默认最大行数范围最小值
    /// </summary>
    int MinRangeConsoleMaxLine { get; set; }

    /// <summary>
    /// 控制台默认最大行数范围最大值
    /// </summary>
    int MaxRangeConsoleMaxLine { get; set; }

    /// <summary>
    /// 控制台最大行数
    /// </summary>
    int ConsoleMaxLine { get; set; }

    /// <summary>
    /// 
    /// </summary>
    int ConsoleFontSizeDefault { get; set; }

    /// <summary>
    /// 
    /// </summary>
    int MinRangeConsoleFontSize { get; set; }

    /// <summary>
    /// 
    /// </summary>
    int MaxRangeConsoleFontSize { get; set; }

    /// <summary>
    /// 控制台字体大小
    /// </summary>
    int ConsoleFontSize { get; set; }

    /// <summary>
    /// 
    /// </summary>
    int IPCPortIdValue { get; set; }

    /// <summary>
    /// IPC 端口号，默认值为 <see cref="DefaultIPCPortIdValue" />
    /// </summary>
    int IPCPortId { get; set; }

    /// <summary>
    /// IPC 端口号被占用时是否随机一个未使用的端口号，默认值 <see langword="true" />
    /// </summary>
    bool IPCPortOccupiedRandom { get; set; }

    /// <summary>
    /// ASF 路径的默认值
    /// </summary>
    static readonly string DefaultArchiSteamFarmExePath = string.Empty;

    /// <summary>
    /// 程序启动时自动运行 ASF的默认值
    /// </summary>
    static readonly bool DefaultAutoRunArchiSteamFarm = false;

    /// <summary>
    /// 检查文件安全性的默认值
    /// </summary>
    static readonly bool DefaultCheckArchiSteamFarmExe = false;

    /// <summary>
    /// 控制台默认最大行数的默认值
    /// </summary>
    const int DefaultConsoleMaxLineDefault = 200;

    /// <summary>
    /// 控制台默认最大行数范围最小值的默认值
    /// </summary>
    const int DefaultMinRangeConsoleMaxLine = DefaultConsoleMaxLineDefault;

    /// <summary>
    /// 控制台默认最大行数范围最大值的默认值
    /// </summary>
    const int DefaultMaxRangeConsoleMaxLine = 5000;

    /// <summary>
    /// 控制台最大行数的默认值
    /// </summary>
    static readonly int DefaultConsoleMaxLine = DefaultConsoleMaxLineDefault;

    /// <summary>
    /// 的默认值
    /// </summary>
    const int DefaultConsoleFontSizeDefault = 14;

    /// <summary>
    /// 的默认值
    /// </summary>
    const int DefaultMinRangeConsoleFontSize = 8;

    /// <summary>
    /// 的默认值
    /// </summary>
    const int DefaultMaxRangeConsoleFontSize = 24;

    /// <summary>
    /// 控制台字体大小的默认值
    /// </summary>
    static readonly int DefaultConsoleFontSize = DefaultConsoleFontSizeDefault;

    /// <summary>
    /// 的默认值
    /// </summary>
    const int DefaultIPCPortIdValue = 6242;

    /// <summary>
    /// IPC 端口号，默认值为 <see cref="DefaultIPCPortIdValue" />的默认值
    /// </summary>
    static readonly int DefaultIPCPortId = DefaultIPCPortIdValue;

    /// <summary>
    /// IPC 端口号被占用时是否随机一个未使用的端口号，默认值 <see langword="true" />的默认值
    /// </summary>
    static readonly bool DefaultIPCPortOccupiedRandom = true;

}
